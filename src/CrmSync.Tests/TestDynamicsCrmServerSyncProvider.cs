using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using CrmAdo;
using CrmSync.Plugin;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.Server;

namespace CrmSync.Tests
{
    public class TestDynamicsCrmServerSyncProvider : DbServerSyncProvider
    {

        public const string TestEntityName = "crmsync_atestpluginentity";
        public const string NameAttributeName = "crmsync_atestpluginentityname";
        public const string IdAttributeName = "crmsync_atestpluginentityid";

        public static Dictionary<string, DbType> AllColumnInfo = new Dictionary<string, DbType>();
        public static Dictionary<string, DbType> SelectColumns = new Dictionary<string, DbType>();

        public static Dictionary<string, DbType> InsertColumns = new Dictionary<string, DbType>();
        public static Dictionary<string, DbType> UpdateColumns = new Dictionary<string, DbType>();


        public static Dictionary<string, DbType> ColumnInfo = new Dictionary<string, DbType>();

        private void LoadColumnInfo()
        {
            // System Fields.
            AllColumnInfo[IdAttributeName] = DbType.Guid;
            AllColumnInfo[NameAttributeName] = DbType.String;

            AllColumnInfo["createdby"] = DbType.Guid;
            AllColumnInfo["createdonbehalfby"] = DbType.Guid;
            AllColumnInfo["modifiedby"] = DbType.Guid;
            AllColumnInfo["modifiedonbehalfby"] = DbType.Guid;
            AllColumnInfo["ownerid"] = DbType.Guid;
            AllColumnInfo["owningbusinessunit"] = DbType.Guid;
            AllColumnInfo["owningteam"] = DbType.Guid;
            AllColumnInfo["owninguser"] = DbType.Guid;
            AllColumnInfo["versionnumber"] = DbType.Int64;

            // Sync System Fields.
            AllColumnInfo[CrmSyncChangeTrackerPlugin.CreatedRowVersionAttributeName] = DbType.Decimal;

            // Custom Fields.

            // Fields for Generated Insert Statement
            InsertColumns[NameAttributeName] = AllColumnInfo[NameAttributeName];

            // Fields for Generated Update Statement
            UpdateColumns[NameAttributeName] = AllColumnInfo[NameAttributeName];

            // Fields for Generated Select Statement.
            foreach (var a in AllColumnInfo)
            {
                SelectColumns[a.Key] = a.Value;
            }

        }

        public TestDynamicsCrmServerSyncProvider(string crmConnectionString)
        {
            LoadColumnInfo();

            var serverConn = new CrmDbConnection(crmConnectionString);
            this.Connection = serverConn;

            // Sql strings for all sync commands.
            var entityName = TestEntityName;
            var idColumn = IdAttributeName;

            var sqlStringGetAnchor = string.Format("select top 1 versionnumber from {0} order by {1} desc", entityName, CrmSyncChangeTrackerPlugin.RowVersionAttributeName);
            // just get records with a creation version greater than last anchor and less than or equal to current anchor.
            var sqlStringGetInserts = string.Format("SELECT {0} FROM {1} WHERE ({2} > @sync_last_received_anchor AND {2} <= @sync_new_received_anchor)", string.Join(",", SelectColumns.Keys), entityName, CrmSyncChangeTrackerPlugin.CreatedRowVersionAttributeName);
            var sqlStringGetUpdates = string.Format("SELECT {0} FROM {1} WHERE (versionnumber > @sync_last_received_anchor AND versionnumber <= @sync_new_received_anchor)", string.Join(",", SelectColumns.Keys), entityName);
            var sqlStringApplyInsert = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", entityName, string.Join(",", InsertColumns.Keys), string.Join(",", InsertColumns.Select(a => "@" + a.Key)));
            var sqlStringApplyUpdate = string.Format("UPDATE {0} SET {1} WHERE ({2} = @{2})", entityName, string.Join(",", UpdateColumns.Select(a => "@" + a.Key)), idColumn);

            DbCommand selectNewAnchorCommand = new AnchorDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);
            var anchorParam = selectNewAnchorCommand.CreateParameter();
            anchorParam.ParameterName = "@" + SyncSession.SyncNewReceivedAnchor;

            selectNewAnchorCommand.CommandText = sqlStringGetAnchor;
            selectNewAnchorCommand.Parameters.Add(anchorParam);
            anchorParam.Direction = ParameterDirection.Output;
            selectNewAnchorCommand.Connection = serverConn;
            this.SelectNewAnchorCommand = selectNewAnchorCommand;

            //Create the SyncAdapter.
            //TODO: Create EntitySyncAdapter subclass of SyncAdapter;
            var customerSyncAdapter = new SyncAdapter(entityName);
            var customerIncrInserts = new SelectIncrementalCreatesCommand(serverConn.CreateCommand() as CrmDbCommand);

            customerIncrInserts.CommandText = sqlStringGetInserts;
            //  "AND crmsync_insertedbyclientid <> @sync_client_id)";

            var lastAnchorParam = customerIncrInserts.CreateParameter();
            lastAnchorParam.ParameterName = "@" + SyncSession.SyncLastReceivedAnchor;
            lastAnchorParam.DbType = DbType.Int64;
            customerIncrInserts.Parameters.Add(lastAnchorParam);

            var thisAnchorParam = customerIncrInserts.CreateParameter();
            thisAnchorParam.ParameterName = "@" + SyncSession.SyncNewReceivedAnchor;
            thisAnchorParam.DbType = DbType.Int64;
            customerIncrInserts.Parameters.Add(thisAnchorParam);

            var clientIdParam = customerIncrInserts.CreateParameter();
            clientIdParam.ParameterName = "@" + SyncSession.SyncClientId;
            clientIdParam.DbType = DbType.Guid;
            customerIncrInserts.Parameters.Add(clientIdParam);

            customerIncrInserts.Connection = serverConn;
            customerSyncAdapter.SelectIncrementalInsertsCommand = customerIncrInserts;

            //Apply inserts to the server.
            // string[] insertParamNames = (from a in InsertColumns select "@" + a.Key).ToArray();

            DbCommand contactInserts = new InsertEntityDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);

            contactInserts.CommandText = sqlStringApplyInsert;
            foreach (var insertColumn in InsertColumns)
            {
                AddParameter(contactInserts, insertColumn.Key, insertColumn.Value);
            }

            AddParameter(contactInserts, SyncSession.SyncClientId, DbType.Guid);
            var param = AddParameter(contactInserts, SyncSession.SyncRowCount, DbType.Int32);
            param.Direction = ParameterDirection.Output;

            contactInserts.Connection = serverConn;
            customerSyncAdapter.InsertCommand = contactInserts;

            //Select updates from the server.
            var customerIncrUpdates = new SelectIncrementalUpdatesCommand(serverConn.CreateCommand() as CrmDbCommand);


            customerIncrUpdates.CommandText = sqlStringGetUpdates;
            AddParameter(customerIncrUpdates, SyncSession.SyncLastReceivedAnchor, DbType.Int64);
            AddParameter(customerIncrUpdates, SyncSession.SyncNewReceivedAnchor, DbType.Int64);
            AddParameter(customerIncrUpdates, SyncSession.SyncClientId, DbType.Guid);
            customerIncrUpdates.Connection = serverConn;
            customerSyncAdapter.SelectIncrementalUpdatesCommand = customerIncrUpdates;

            var customerUpdates = new UpdateEntityDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);

            customerUpdates.CommandText = sqlStringApplyUpdate;
            foreach (var col in UpdateColumns)
            {
                AddParameter(contactInserts, col.Key, col.Value);
            }

            AddParameter(customerUpdates, SyncSession.SyncClientId, DbType.Guid);
            AddParameter(customerUpdates, SyncSession.SyncForceWrite, DbType.Boolean);
            AddParameter(customerUpdates, SyncSession.SyncLastReceivedAnchor, DbType.Int64);
            AddParameter(customerUpdates, SyncSession.SyncRowCount, DbType.Int32);

            customerUpdates.Connection = serverConn;
            customerSyncAdapter.UpdateCommand = customerUpdates;

            //Add the SyncAdapter to the server synchronization provider.
            this.SyncAdapters.Add(customerSyncAdapter);

        }

        private static DbParameter AddParameter(DbCommand command, string parameterName, DbType dbType)
        {
            var par = command.CreateParameter();
            par.ParameterName = "@" + parameterName;
            par.DbType = dbType;
            command.Parameters.Add(par);
            return par;
        }

        public void InsertNewRecord()
        {

            var valuesForInsert = new Dictionary<string, string>();
            var valuesClause = Utility.BuildSqlValuesClause(InsertColumns, valuesForInsert);
            var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                            TestEntityName,
                                            string.Join(",", InsertColumns.Keys),
                                            valuesClause);

            int rowCount = 0;
            using (var serverConn = this.Connection)
            {
                serverConn.Open();
                // An insert..
                using (var command = serverConn.CreateCommand())
                {
                    command.CommandText = commandText;
                    rowCount = command.ExecuteNonQuery();
                }
                serverConn.Close();
            }

            Console.WriteLine("Rows inserted, updated, or deleted at the server: " + rowCount);
        }
    }
}