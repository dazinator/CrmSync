using System;
using System.Data;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.SqlServerCe;

namespace CrmSync
{
    public class SampleClientSyncProvider : SqlCeClientSyncProvider
    {

        public SampleClientSyncProvider()
        {
            //Specify a connection string for the sample client database.
            Utility util = new Utility();
            this.ConnectionString = Utility.ConnStr_SqlCeClientSync;

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
            Utility util = new Utility();
            Utility.MakeSchemaChangesOnClient(e.Connection, e.Transaction, e.Table.TableName);
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


    }
}