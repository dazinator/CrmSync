using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;

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


        public TestDynamicsCrmServerSyncProvider DynamicsSyncProvider { get { return this.RemoteProvider as TestDynamicsCrmServerSyncProvider; } }

        public TestClientSyncProvider ClientSyncProvider { get { return this.LocalProvider as TestClientSyncProvider; } }



    }
}