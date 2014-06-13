using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace CrmSync
{
    public class CrmSyncChangeTrackerPlugin : BasePlugin
    {

       // public const string PluginName = "CrmSyncChangeTrackerPlugin";

        public const string RowVersionAttributeName = "rowversion";
        public const string CreatedRowVersionAttributeName = "crmsync_createdrowversion";

        protected override void Execute()
        {
            EnsureTransaction();
            var targetEntity = EnsureTargetEntity();

            if (!targetEntity.Contains(RowVersionAttributeName))
            {
                Fail("Could not get the RowVersion of the Target Entity");
                return;
            }

            var rowVersion = targetEntity[RowVersionAttributeName];
            targetEntity[CreatedRowVersionAttributeName] = rowVersion;
            var orgService = GetOrganisationService();
            orgService.Update(targetEntity);

        }
    }


}
