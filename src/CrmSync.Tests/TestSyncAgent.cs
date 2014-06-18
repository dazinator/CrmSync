using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrmAdo;
using CrmSync.Dynamics;
using CrmSync.Plugin;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.Server;
using Microsoft.Synchronization.Data.SqlServerCe;

namespace CrmSync.Tests
{
    public class TestDynamicsCrmSyncAgent : SyncAgent
    {

        public TestDynamicsCrmSyncAgent(string sqlCompactConnectionString, string crmConnectionString)
        {
            //Instantiate a client synchronization provider and specify it
            //as the local provider for this synchronization agent.
            this.LocalProvider = new TestClientSyncProvider(sqlCompactConnectionString);

            //Instantiate a server synchronization provider and specify it
            //as the remote provider for this synchronization agent.
            this.RemoteProvider = new TestDynamicsCrmServerSyncProvider(crmConnectionString);

            //Create a Customer SyncGroup. This is not required
            //for the single table we are synchronizing; it is typically
            //used so that changes to multiple related tables are 
            //synchronized at the same time.
            SyncGroup customerSyncGroup = new SyncGroup("dynamics");

            //Add the Customer table: specify a synchronization direction of
            //Bidirectional, and that an existing table should be dropped.
            SyncTable customerSyncTable = new SyncTable(TestDynamicsCrmServerSyncProvider.TestEntityName);
            customerSyncTable.CreationOption = TableCreationOption.DropExistingOrCreateNewTable;
            customerSyncTable.SyncDirection = SyncDirection.Bidirectional;
            customerSyncTable.SyncGroup = customerSyncGroup;
            this.Configuration.SyncTables.Add(customerSyncTable);
        }

        public virtual void ProvisionEntity(string logicalEntityName)
        {
            //            This will involve:-

            //Ensuring the entity exists.
            //Ensuring the entity has additional custom fields created that are required for Sync Process
            //crmsync_createdrowversion attribute needs to exist.
            //Registering a plugin for #1 with correct plugin step to run post create.

        }
        
    }

    public class TestDynamicsCrmServerSyncProvider : DbServerSyncProvider
    {

        public const string TestEntityName = "crmsync_testpluginentity";
        public const string NameAttributeName = "crmsync_testpluginentityname";
        public const string IdAttributeName = "crmsync_testpluginentityid";

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
            var sqlStringGetInserts = string.Format("SELECT {0} FROM {1} WHERE (versionnumber > @sync_last_received_anchor AND versionnumber <= @sync_new_received_anchor)", string.Join(",", SelectColumns.Keys), entityName);
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
            var customerSyncAdapter = new SyncAdapter(entityName);
            var customerIncrInserts = new SelectIncrementalChangesDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);

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
            var customerIncrUpdates = new SelectIncrementalChangesDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);


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

    public class TestClientSyncProvider : SqlCeClientSyncProvider
    {

        public TestClientSyncProvider(string connectionString)
        {
            this.ConnectionString = connectionString;

            //We use the CreatingSchema event to change the schema
            //by using the API. We use the SchemaCreated event to 
            //change the schema by using SQL.
            this.CreatingSchema += new EventHandler<CreatingSchemaEventArgs>(SampleClientSyncProvider_CreatingSchema);
            this.SchemaCreated += new EventHandler<SchemaCreatedEventArgs>(SampleClientSyncProvider_SchemaCreated);
            this.ApplyChangeFailed += SampleClientSyncProvider_ApplyChangeFailed;

        }

        private void SampleClientSyncProvider_CreatingSchema(object sender, CreatingSchemaEventArgs e)
        {
            //Set the RowGuid property because it is not copied
            //to the client by default. This is also a good time
            //to specify literal defaults with .Columns[ColName].DefaultValue;
            //but we will specify defaults like NEWID() by calling
            //ALTER TABLE after the table is created.
            Console.Write("Creating schema for " + e.Table.TableName + " | ");
            var idColumn = e.Table.TableName + "id";
            e.Schema.Tables[e.Table.TableName].Columns[idColumn].RowGuid = true;
        }

        private void SampleClientSyncProvider_SchemaCreated(object sender, SchemaCreatedEventArgs e)
        {
            //Call ALTER TABLE on the client. This must be done
            //over the same connection and within the same
            //transaction that Sync Framework uses
            //to create the schema on the client.
            InitialiseSchema(e.Connection, e.Transaction, e.Table.TableName);
            Console.WriteLine("Schema created for " + e.Table.TableName);
        }

        protected override void OnApplyChangeFailed(ApplyChangeFailedEventArgs value)
        {
            Console.Write("applying changes failed..");
            base.OnApplyChangeFailed(value);
        }

        public override SyncContext ApplyChanges(SyncGroupMetadata groupMetadata, System.Data.DataSet dataSet, SyncSession syncSession)
        {
            Console.Write("apply changes on client..");
            //PrintDataSet(dataSet);
            var context = base.ApplyChanges(groupMetadata, dataSet, syncSession);
            return context;
        }

        protected override void OnApplyingChanges(ApplyingChangesEventArgs value)
        {
            Console.Write("on applying changes on client..");
            base.OnApplyingChanges(value);
        }

        protected override void OnChangesApplied(ChangesAppliedEventArgs value)
        {
            Console.Write("on changes applied on client..");
            base.OnChangesApplied(value);
        }

        protected override void OnSyncProgress(SyncProgressEventArgs value)
        {
            Console.Write("on sync progress on client..");
            base.OnSyncProgress(value);

        }

        void SampleClientSyncProvider_ApplyChangeFailed(object sender, ApplyChangeFailedEventArgs e)
        {
            Console.Write("APPLYING CHANGES FAILED..");

        }

        static void PrintDataSet(DataSet ds)
        {
            Console.WriteLine("Tables in '{0}' DataSet.\n", ds.DataSetName);
            foreach (DataTable dt in ds.Tables)
            {
                Console.WriteLine("{0} Table.\n", dt.TableName);
                for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                {
                    //Console.Write(dt.Columns[curCol].ColumnName.Trim() + "\t");
                }
                for (int curRow = 0; curRow < dt.Rows.Count; curRow++)
                {
                    var row = dt.Rows[curRow];
                    Console.WriteLine("Row state: " + row.RowState.ToString());
                    for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                    {
                        var col = row[curCol];
                        Console.Write(col.ToString().Trim() + "\t");
                    }
                    Console.WriteLine();
                }
            }
        }

        //Add DEFAULT constraints on the client.
        public static void InitialiseSchema(IDbConnection clientConn, IDbTransaction clientTran, string tableName)
        {
            //Execute the command over the same connection and within
            //the same transaction that Sync Framework uses
            //to create the schema on the client.
            var alterTable = new SqlCeCommand();
            alterTable.Connection = (SqlCeConnection)clientConn;
            alterTable.Transaction = (SqlCeTransaction)clientTran;
            alterTable.CommandText = String.Empty;

            //Execute the command, then leave the transaction and 
            //connection open. The client provider will commit and close.
            switch (tableName)
            {
                case TestDynamicsCrmServerSyncProvider.TestEntityName:
                    var idColumnName = TestDynamicsCrmServerSyncProvider.IdAttributeName;

                    alterTable.CommandText =
                        "ALTER TABLE " + TestDynamicsCrmServerSyncProvider.TestEntityName +
                        " ADD CONSTRAINT DF_" + idColumnName +
                        " DEFAULT NEWID() FOR " + idColumnName;
                    alterTable.ExecuteNonQuery();
                    break;
            }
        }


    }
}
