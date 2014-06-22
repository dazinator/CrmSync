using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmSync.Plugin
{
    public class CrmSyncChangeTrackerPlugin : BasePlugin, IPlugin
    {
        public CrmSyncChangeTrackerPlugin()
        {

        }
        // public const string PluginName = "CrmSyncChangeTrackerPlugin";

     

        protected override void Execute()
        {
            EnsureTransaction();

            var targetEntity = EnsureTargetEntity();
            IOrganizationService orgService = null;

            if (!targetEntity.Contains(SyncColumnInfo.RowVersionAttributeName))
            {
                // retrieve from db again?
                orgService = GetOrganisationService();
                //Entity currentEntity;
                try
                {
                    targetEntity = orgService.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet(SyncColumnInfo.RowVersionAttributeName));
                }
                catch (Exception e)
                {
                    Fail("Could not retreive entity in post operation.");
                    return;
                }

                if (!targetEntity.Contains(SyncColumnInfo.RowVersionAttributeName))
                {
                    Fail("Could not get the RowVersion of the current Entity");
                    return;
                }
            }

            var rowVersion = targetEntity[SyncColumnInfo.RowVersionAttributeName];
            var capturedRowVersion = Convert.ToDecimal(rowVersion);

            targetEntity[SyncColumnInfo.CreatedRowVersionAttributeName] = capturedRowVersion;
            if (orgService == null)
            {
                orgService = GetOrganisationService();
            }
            orgService.Update(targetEntity);

        }
    }


}
