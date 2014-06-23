using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using CrmAdo;
using CrmSync.Plugin;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.Server;
using Microsoft.Xrm.Sdk.Metadata;

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

        public static string TestDatetimeColumnName = TestEntityName + "datetime";
        public static string TestDateOnlyColumnName = TestEntityName + "dateonly";

        public static string DecimalColumnNamePrefix = TestEntityName + "dec";
        public static string MoneyColumnNamePrefix = TestEntityName + "mon";
        public static string BoolColumnName = TestEntityName + "bool";
        public static string IntColumnName = TestEntityName + "int";

        public static string MemoColumnName = TestEntityName + "memo";
        public static string PicklistColumnName = TestEntityName + "picklist";


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

            var createdByColumn = new ColumnInfo("createdby", DbType.Guid);
            AllColumnInfo.Add(createdByColumn);

            var createdOnColumn = new ColumnInfo("createdon", DbType.DateTime);
            AllColumnInfo.Add(createdByColumn);

            var createdOnBehalfByColumn = new ColumnInfo("createdonbehalfby", DbType.Guid);
            AllColumnInfo.Add(createdOnBehalfByColumn);

            var modifiedByColumn = new ColumnInfo("modifiedby", DbType.Guid);
            AllColumnInfo.Add(modifiedByColumn);

            var modifiedOnBehalfByColumn = new ColumnInfo("modifiedonbehalfby", DbType.Guid);
            AllColumnInfo.Add(modifiedOnBehalfByColumn);

            var ownerIdColumn = new ColumnInfo("ownerid", DbType.Guid);
            AllColumnInfo.Add(ownerIdColumn);

            var owningBusinessUnitColumn = new ColumnInfo("owningbusinessunit", DbType.Guid);
            AllColumnInfo.Add(owningBusinessUnitColumn);

            var owningTeamColumn = new ColumnInfo("owningteam", DbType.Guid);
            AllColumnInfo.Add(owningTeamColumn);

            var owningUserColumn = new ColumnInfo("owninguser", DbType.Guid);
            AllColumnInfo.Add(owningUserColumn);

            var versionNumberColumn = new ColumnInfo("versionnumber", DbType.Int64);
            AllColumnInfo.Add(versionNumberColumn);

            // Sync provisioned fields.
            var createdBySyncClientIdColumn = new ColumnInfo(SyncColumnInfo.CreatedBySyncClientIdAttributeName, DbType.Guid, SyncSession.SyncClientId);
            AllColumnInfo.Add(createdBySyncClientIdColumn);

            var createdRowVersionColumn = new ColumnInfo(SyncColumnInfo.CreatedRowVersionAttributeName, DbType.Decimal);
            AllColumnInfo.Add(createdRowVersionColumn);

            //Custom fields of various types.

            // Date and time
            var testDateTimeColumn = new ColumnInfo(TestDatetimeColumnName, DbType.DateTime);
            AllColumnInfo.Add(testDateTimeColumn);

            // Date only
            var testDateOnlyColumn = new ColumnInfo(TestDateOnlyColumnName, DbType.Date);
            AllColumnInfo.Add(testDateOnlyColumn);

            // Decimal columns
            // Loop through supported decimal precision and create a decimal attribute for each precision.
            for (int i = DecimalAttributeMetadata.MinSupportedPrecision; i <= DecimalAttributeMetadata.MaxSupportedPrecision; i++)
            {
                var decColumn = new ColumnInfo(TestDynamicsCrmServerSyncProvider.DecimalColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture), DbType.Decimal);
                AllColumnInfo.Add(decColumn);
            }

            // Money Columns
            // Loop through supported money precision and create a money attribute for each precision.
            for (int i = MoneyAttributeMetadata.MinSupportedPrecision; i <= MoneyAttributeMetadata.MaxSupportedPrecision; i++)
            {
                var monColumn = new ColumnInfo(TestDynamicsCrmServerSyncProvider.MoneyColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture), DbType.Currency);
                AllColumnInfo.Add(monColumn);
            }

            // Bool
            var boolColumn = new ColumnInfo(TestDynamicsCrmServerSyncProvider.BoolColumnName, DbType.Int32);
            AllColumnInfo.Add(boolColumn);

            //// int
            //// Add in all possible integer formats.
            //var enumVals = Enum.GetValues(typeof(IntegerFormat));
            //foreach (var enumVal in enumVals)
            //{
            //    IntegerFormat format = (IntegerFormat)enumVal;
            //    string formatName = format.ToString();
            //    var attName = TestDynamicsCrmServerSyncProvider.IntColumnName + formatName;

            //    var intColumn = new ColumnInfo(attName, DbType.Int32);
            //    AllColumnInfo.Add(intColumn);

            //}

            // memo
            var memoColumn = new ColumnInfo(TestDynamicsCrmServerSyncProvider.MemoColumnName, DbType.String);
            AllColumnInfo.Add(memoColumn);


            // picklist
            var picklistColumn = new ColumnInfo(TestDynamicsCrmServerSyncProvider.PicklistColumnName, DbType.Int32);
            AllColumnInfo.Add(picklistColumn);


            // Fields to be included in server insert statement.
            InsertColumns.Add(idColumn);
            InsertColumns.Add(nameColumn);
            InsertColumns.Add(createdBySyncClientIdColumn);

            // Fields to be included in client insert statement.
            ClientInsertColumns.Add(nameColumn);

            // Fields to be included in server update statement.
            UpdateColumns.Add(nameColumn);

            // Fields to be selected from the server and replicated to the client.

            SelectColumns.Add(idColumn);
            SelectColumns.Add(nameColumn);
            SelectColumns.Add(createdByColumn);
            SelectColumns.Add(createdOnBehalfByColumn);
            SelectColumns.Add(modifiedByColumn);
            SelectColumns.Add(modifiedOnBehalfByColumn);
            SelectColumns.Add(ownerIdColumn);
            SelectColumns.Add(owningBusinessUnitColumn);
            SelectColumns.Add(owningTeamColumn);
            SelectColumns.Add(owningUserColumn);
            SelectColumns.Add(versionNumberColumn);
            SelectColumns.Add(createdBySyncClientIdColumn);
            //  SelectColumns.Add(createdRowVersionColumn);

            SelectColumns.Add(createdOnColumn);

            // Specific fields.
            // dates
            SelectColumns.Add(testDateTimeColumn);
            SelectColumns.Add(testDateOnlyColumn);

            // decimal columns
            for (int i = DecimalAttributeMetadata.MinSupportedPrecision; i <= DecimalAttributeMetadata.MaxSupportedPrecision; i++)
            {
                var name = TestDynamicsCrmServerSyncProvider.DecimalColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture);
                var existingCol = AllColumnInfo.Single(c => c.AttributeName == name);
                SelectColumns.Add(existingCol);
            }

            // money columns
            for (int i = MoneyAttributeMetadata.MinSupportedPrecision; i <= MoneyAttributeMetadata.MaxSupportedPrecision; i++)
            {
                var name = TestDynamicsCrmServerSyncProvider.MoneyColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture);
                var existingCol = AllColumnInfo.Single(c => c.AttributeName == name);
                SelectColumns.Add(existingCol);
            }

            // bool
            SelectColumns.Add(boolColumn);

            //// int
            //foreach (var enumVal in enumVals)
            //{
            //    IntegerFormat format = (IntegerFormat)enumVal;
            //    string formatName = format.ToString();
            //    var attName = TestDynamicsCrmServerSyncProvider.IntColumnName + formatName;

            //    var existingCol = AllColumnInfo.Single(c => c.AttributeName == attName);
            //    SelectColumns.Add(existingCol);

            //}

            // memo
            SelectColumns.Add(memoColumn);

            // picklist
            SelectColumns.Add(picklistColumn);
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