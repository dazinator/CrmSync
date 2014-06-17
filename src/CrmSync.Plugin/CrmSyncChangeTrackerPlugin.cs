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

        public const string RowVersionAttributeName = "versionnumber";
        public const string CreatedRowVersionAttributeName = "crmsync_createdversionnumber";

        protected override void Execute()
        {
            EnsureTransaction();
           
            var targetEntity = EnsureTargetEntity();
            IOrganizationService orgService = null;

            if (!targetEntity.Contains(RowVersionAttributeName))
            {
                // retrieve from db again?
                orgService = GetOrganisationService();
                //Entity currentEntity;
                try
                {
                    targetEntity = orgService.Retrieve(targetEntity.LogicalName, targetEntity.Id, new ColumnSet(RowVersionAttributeName));
                }
                catch (Exception e)
                {
                    Fail("Could not retreive entity in post operation.");
                    return;
                }

                if (!targetEntity.Contains(RowVersionAttributeName))
                {
                    Fail("Could not get the RowVersion of the current Entity");
                    return;
                }
            }

            var rowVersion = targetEntity[RowVersionAttributeName];
            var capturedRowVersion = Convert.ToDecimal(rowVersion);

            targetEntity[CreatedRowVersionAttributeName] = capturedRowVersion;
            if (orgService == null)
            {
                orgService = GetOrganisationService();
            }
            orgService.Update(targetEntity);

        }
    }


}
