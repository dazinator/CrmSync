using System;
using System.Configuration;
using System.Linq;
using CrmDeploy;
using CrmDeploy.Enums;
using CrmSync.Dynamics;
using CrmSync.Dynamics.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;
using CrmSync.Plugin;

namespace CrmSync.Tests.SystemTests
{  
    [Category("Plugin")]
    [TestFixture]
    public class CrmSyncPluginSystemTests : CrmSyncIntegrationTest
    {     

        public CrmSyncPluginSystemTests()
        {

        }


        [Test]
        public void Crm_Plugin_Captures_Creation_Version_On_Create_Of_Entity()
        {
            // create a new entity.

            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            using (var orgServiceContext = (OrganizationServiceContext)service.GetOrganisationService())
            {
                // Create a new entity record which should fire plugin in crm.
                var testEntity = new Entity(this.TestEntityLogicalName);
                var nameAttribute = this.TestEntityMetadata.PrimaryNameAttribute;
                testEntity[nameAttribute] = "you shall pass!";
                var orgService = (IOrganizationService)orgServiceContext;
                var newRecordId = orgService.Create(testEntity);

                Console.WriteLine("Record created: " + newRecordId);

                // pull back the record and verify the plugin captured the creation version of the record.
                var ent = orgService.Retrieve(this.TestEntityLogicalName, newRecordId, new ColumnSet(SyncColumnInfo.RowVersionAttributeName, SyncColumnInfo.CreatedRowVersionAttributeName));

                Assert.That(ent.Attributes.ContainsKey(SyncColumnInfo.CreatedRowVersionAttributeName));
                Assert.That(ent.Attributes.ContainsKey(SyncColumnInfo.RowVersionAttributeName));

                var rowVersion = (long)ent.Attributes[SyncColumnInfo.RowVersionAttributeName];
                Assert.That(rowVersion, Is.GreaterThan(0));

                var capturedCreationVersion = System.Convert.ToInt64((decimal)ent.Attributes[SyncColumnInfo.CreatedRowVersionAttributeName]);

                Assert.That(capturedCreationVersion, Is.GreaterThan(0));
                // the row version is incremented on every modification of the record - as the plugin modified it when saving the creation version this causes the
                // row version to be atleast 1 greater than creation version.
                // If many tests are executing in dynamics at the same time then the rowversion could be incrememnted by more than 1 as its globally unique.
                // so we make an allowance here of max 5 difference.
                Assert.That(rowVersion - capturedCreationVersion, Is.LessThan(5));
                Assert.That(rowVersion - capturedCreationVersion, Is.GreaterThan(-1));

                Console.WriteLine("Creation version is: " + capturedCreationVersion);
                Console.WriteLine("Row version is: " + rowVersion);

            }



        }


    }

}
