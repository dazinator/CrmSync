﻿using System;
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

    public class ColumnInfo
    {
        public ColumnInfo(string attributeName, DbType type)
        {
            AttributeName = attributeName;
            Type = type;
            BoundParameterName = string.Format("@{0}", attributeName);
        }

        public ColumnInfo(string attributeName, DbType type, string boundParameterName)
        {
            AttributeName = attributeName;
            Type = type;

            if (!boundParameterName.StartsWith("@"))
            {
                BoundParameterName = string.Format("@{0}", boundParameterName);
            }
            else
            {
                BoundParameterName = boundParameterName;
            }

        }


        public string AttributeName { get; set; }
        public string BoundParameterName { get; set; }
        public DbType Type { get; set; }
    }


    public class TestDynamicsCrmServerSyncProvider : DbServerSyncProvider
    {

        public static string TestEntityName = ("crmsync_" + (char)new Random().Next(128)).ToLower();
        public static string NameAttributeName = TestEntityName + "name";
        public static string IdAttributeName = TestEntityName + "id";

        public static List<ColumnInfo> AllColumnInfo = new List<ColumnInfo>();
        public static List<ColumnInfo> SelectColumns = new List<ColumnInfo>();

        public static List<ColumnInfo> InsertColumns = new List<ColumnInfo>();
        public static List<ColumnInfo> UpdateColumns = new List<ColumnInfo>();

        public static List<ColumnInfo> ClientInsertColumns = new List<ColumnInfo>();

        private void LoadColumnInfo()
        {
            // Crm System Fields.
            var idColumn = new ColumnInfo(IdAttributeName, DbType.Guid);
            AllColumnInfo.Add(idColumn);

            var nameColumn = new ColumnInfo(NameAttributeName, DbType.String);
            AllColumnInfo.Add(nameColumn);

            AllColumnInfo.Add(new ColumnInfo("createdby", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("createdonbehalfby", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("modifiedby", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("modifiedonbehalfby", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("ownerid", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("owningbusinessunit", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("owningteam", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("owninguser", DbType.Guid));
            AllColumnInfo.Add(new ColumnInfo("versionnumber", DbType.Int64));

            // Sync provisioned fields.
            var createdBySyncClientIdColumn = new ColumnInfo(SyncColumnInfo.CreatedBySyncClientIdAttributeName, DbType.Guid, SyncSession.SyncClientId);
            AllColumnInfo.Add(createdBySyncClientIdColumn);

            var createdRowVersionColumn = new ColumnInfo(SyncColumnInfo.CreatedRowVersionAttributeName, DbType.Decimal);
            AllColumnInfo.Add(createdRowVersionColumn);

            // Fields to be included in server insert statement.
            InsertColumns.Add(idColumn);
            InsertColumns.Add(nameColumn);
            InsertColumns.Add(createdBySyncClientIdColumn);

            // Fields to be included in client insert statement.
            ClientInsertColumns.Add(nameColumn);

            // Fields to be included in server update statement.
            UpdateColumns.Add(nameColumn);

            // Fields to be selected from the server and replicated to the client.
            foreach (var a in AllColumnInfo)
            {
                // dont include the client id in selection.
                if (a.AttributeName != SyncColumnInfo.CreatedBySyncClientIdAttributeName)
                {
                    SelectColumns.Add(a);
                }
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

            var sqlStringGetAnchor = string.Format("select top 1 {0} from {1} order by {0} desc", SyncColumnInfo.RowVersionAttributeName, entityName);
            // just get records with a creation version greater than last anchor and less than or equal to current anchor.
            var selectInsertsColumns = string.Join(",", SelectColumns.Select(s => s.AttributeName));


            var sqlStringGetInserts = string.Format("SELECT {0} FROM {1} WHERE ({2} > @{3} AND {2} <= @{4}) AND ({5} <> @{6})", selectInsertsColumns, entityName, SyncColumnInfo.CreatedRowVersionAttributeName, SyncSession.SyncLastReceivedAnchor, SyncSession.SyncNewReceivedAnchor, SyncColumnInfo.CreatedBySyncClientIdAttributeName, SyncSession.SyncClientId);

            var sqlSelectUpdateColumns = string.Join(",", SelectColumns.Select(s => s.AttributeName));
            var sqlStringGetUpdates = string.Format("SELECT {0} FROM {1} WHERE ({2} > @{3} AND {2} <= @{4})", sqlSelectUpdateColumns, entityName, SyncColumnInfo.RowVersionAttributeName, SyncSession.SyncLastReceivedAnchor, SyncSession.SyncNewReceivedAnchor);


            var sqlInsertColumns = string.Join(",", InsertColumns.Select(s => s.AttributeName));
            var sqlInsertParameters = string.Join(",", InsertColumns.Select(a => a.BoundParameterName));
            var sqlStringApplyInsert = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", entityName, sqlInsertColumns, sqlInsertParameters);

            var sqlUpdateColumns = string.Join(",", UpdateColumns.Select(a => a.AttributeName));
            var sqlStringApplyUpdate = string.Format("UPDATE {0} SET {1} WHERE ({2} = @{2})", entityName, sqlUpdateColumns, idColumn);

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
                AddParameter(contactInserts, insertColumn.BoundParameterName, insertColumn.Type);
            }

            // AddParameter(contactInserts, SyncSession.SyncClientId, DbType.Guid);
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
                AddParameter(contactInserts, col.BoundParameterName, col.Type);
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
            if (!parameterName.StartsWith("@"))
            {
                par.ParameterName = "@" + parameterName;
            }
            else
            {
                par.ParameterName = parameterName;
            }
            par.DbType = dbType;
            command.Parameters.Add(par);
            return par;
        }

        public void InsertNewRecord()
        {

            var valuesForInsert = new Dictionary<string, string>();

            var valuesClause = Utility.BuildSqlValuesClause(InsertColumns, valuesForInsert);
            var insertColumnNames = string.Join(",", InsertColumns.Select(s => s.AttributeName));

            var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                            TestEntityName, insertColumnNames, valuesClause);

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