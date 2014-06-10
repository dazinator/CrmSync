using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;

namespace CrmSync
{
    public class DynamicsCrmSyncAgent : SyncAgent
    {
        public DynamicsCrmSyncAgent()
        {
            //Instantiate a client synchronization provider and specify it
            //as the local provider for this synchronization agent.
            this.LocalProvider = new SampleClientSyncProvider();

            //Instantiate a server synchronization provider and specify it
            //as the remote provider for this synchronization agent.
            this.RemoteProvider = new DynamicsCrmServerSyncProvider();

            //Create a Customer SyncGroup. This is not required
            //for the single table we are synchronizing; it is typically
            //used so that changes to multiple related tables are 
            //synchronized at the same time.
            SyncGroup customerSyncGroup = new SyncGroup("dynamics");

            //Add the Customer table: specify a synchronization direction of
            //Bidirectional, and that an existing table should be dropped.
            SyncTable customerSyncTable = new SyncTable(DynamicsCrmServerSyncProvider.EntityName);
            customerSyncTable.CreationOption = TableCreationOption.DropExistingOrCreateNewTable;
            customerSyncTable.SyncDirection = SyncDirection.Bidirectional;
            customerSyncTable.SyncGroup = customerSyncGroup;
            this.Configuration.SyncTables.Add(customerSyncTable);
        }

        public virtual void AddEntity(string logicalEntityName)
        {
            //            This will involve:-

            //Ensuring the entity exists.
            //Ensuring the entity has additional custom fields created that are required for Sync Process
            //crmsync_createdrowversion attribute needs to exist.
            //Registering a plugin for #1 with correct plugin step to run post create.

        }


    }
}