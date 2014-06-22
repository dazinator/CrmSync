using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using CrmSync.Dynamics;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServerCe;

namespace CrmSync.Tests
{
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
           PrintDataSet(e.Schema.SchemaDataSet);

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
            PrintDataSet(dataSet);
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
                //for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                //{
                //    //Console.Write(dt.Columns[curCol].ColumnName.Trim() + "\t");
                //}
                for (int curRow = 0; curRow < dt.Rows.Count; curRow++)
                {
                    var row = dt.Rows[curRow];
                    Console.WriteLine("Row state: " + row.RowState.ToString());
                    for (int curCol = 0; curCol < dt.Columns.Count; curCol++)
                    {
                        var col = row[curCol];

                        Console.Write("{0}: {1}", dt.Columns[curCol].ColumnName, col.ToString().Trim() + "\t");

                        //Console.Write(col.ToString().Trim() + "\t");
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
            if (tableName == TestDynamicsCrmServerSyncProvider.TestEntityName)
            {
                var idColumnName = TestDynamicsCrmServerSyncProvider.IdAttributeName;

                alterTable.CommandText =
                    "ALTER TABLE " + TestDynamicsCrmServerSyncProvider.TestEntityName +
                    " ADD CONSTRAINT DF_" + idColumnName +
                    " DEFAULT NEWID() FOR " + idColumnName;
                alterTable.ExecuteNonQuery();
            }

        }


    }
}
