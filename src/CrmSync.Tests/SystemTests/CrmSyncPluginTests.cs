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
    [Category("System")]
    [Category("Crm Plugin")]
    [TestFixture]
    public class CrmSyncPluginSystemTests
    {
        public const string TestEntityName = "crmsync_testpluginentity";
        public const string NameAttributeName = "crmsync_testpluginentityname";

        public CrmSyncPluginSystemTests()
        {

        }

        public RegistrationInfo PluginRegistrationInfo { get; set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            // Ensure custom test entity present in crm.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            CreateTestEntity(service);

            // Ensure Plugin Registered in CRM.
            RegisterPlugin(service);

        }

        [Test]
        public void Crm_Plugin_Captures_Creation_Version_On_Create_Of_Entity()
        {
            // create a new entity.

            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            using (var orgServiceContext = (OrganizationServiceContext)service.GetOrganisationService())
            {
                // Create a new entity record which should fire plugin in crm.
                var testEntity = new Entity(TestEntityName);
                testEntity[NameAttributeName] = "you shall pass!";
                var orgService = (IOrganizationService)orgServiceContext;
                var newRecordId = orgService.Create(testEntity);


                Console.WriteLine("Record created: " + newRecordId);

                // pull back the record and verify the plugin captured the creation version of the record.
                var ent = orgService.Retrieve(TestEntityName, newRecordId, new ColumnSet(SyncColumnInfo.RowVersionAttributeName, SyncColumnInfo.CreatedRowVersionAttributeName));

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

        [TestFixtureTearDown]
        public void TearDown()
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());

            UnregisterPlugin(service);
            DeleteTestEntity(service);
        }

        private void RegisterPlugin(CrmServiceProvider serviceProvider)
        {
            var orgConnectionString = ConfigurationManager.ConnectionStrings["CrmOrganisationService"];

            var deployer = DeploymentBuilder.CreateDeployment()
                                                           .ForTheAssemblyContainingThisPlugin<CrmSyncChangeTrackerPlugin>("Test plugin assembly")
                                                            .RunsInSandboxMode()
                                                            .RegisterInDatabase()
                                                           .HasPlugin<CrmSyncChangeTrackerPlugin>()
                                                            .WhichExecutesOn(SdkMessageNames.Create, TestEntityName)
                                                            .Synchronously()
                                                            .PostOperation()
                                                            .OnlyOnCrmServer()
                                                           .DeployTo(orgConnectionString.ConnectionString);

            PluginRegistrationInfo = deployer.Deploy();
            if (!PluginRegistrationInfo.Success)
            {
                Assert.Fail("Registration failed..");
            }
        }

        private void UnregisterPlugin(CrmServiceProvider service)
        {
            PluginRegistrationInfo.Undeploy();
        }

        /// <summary>
        /// Ensures test entity is created in CRM.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void CreateTestEntity(ICrmServiceProvider serviceProvider)
        {
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                // Check for test entity - if it doesn't exist then create it.
                var request = new RetrieveEntityRequest();
                request.RetrieveAsIfPublished = true;
                request.LogicalName = TestEntityName;
                RetrieveEntityResponse response = null;
                try
                {
                    response = (RetrieveEntityResponse)orgService.Execute(request);
                }
                catch (Exception e)
                {
                    if (e.Message.ToLower().StartsWith("could not find"))
                    {
                        response = null;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (response == null || response.EntityMetadata == null)
                {
                    var createRequest = new CreateEntityRequest();


                    var entityBuilder = EntityConstruction.ConstructEntity(TestEntityName);
                    createRequest.Entity = entityBuilder
                             .Description("Sync Plugin Test")
                             .DisplayCollectionName("Sync Plugin Test Entities")
                             .DisplayName("Sync Plugin Test")
                             .WithAttributes()
                             .StringAttribute(NameAttributeName, "name", "name attribute", AttributeRequiredLevel.Recommended, 255, StringFormat.Text)
                             .DecimalAttribute(SyncColumnInfo.CreatedRowVersionAttributeName,
                                              "CrmSync Creation Version",
                                              "The RowVersion of the record when it was created.",
                                              AttributeRequiredLevel.None, 0, null, 0)
                             .MetaDataBuilder.Build();

                    //  createRequest.HasActivities = false;
                    //  createRequest.HasNotes = false;
                    createRequest.PrimaryAttribute = (StringAttributeMetadata)entityBuilder.AttributeBuilder.Attributes[0];
                    //  createRequest.SolutionUniqueName =

                    try
                    {
                        var createResponse = (CreateEntityResponse)orgService.Execute(createRequest);
                        foreach (var att in entityBuilder.AttributeBuilder.Attributes.Where(a => a.SchemaName != NameAttributeName))
                        {
                            var createAttributeRequest = new CreateAttributeRequest
                            {
                                EntityName = entityBuilder.Entity.LogicalName,
                                Attribute = att
                            };
                            var createAttResponse = (CreateAttributeResponse)orgService.Execute(createAttributeRequest);
                        }
                    }
                    catch (Exception e)
                    {
                        throw;
                    }



                }

            }
        }

        /// <summary>
        /// Ensures test entity is deleted from CRM.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void DeleteTestEntity(ICrmServiceProvider serviceProvider)
        {
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                // Check for test entity - if it doesn't exist then create it.
                var request = new DeleteEntityRequest();
                request.LogicalName = TestEntityName;
                var response = (DeleteEntityResponse)orgService.Execute(request);
                if (response == null)
                {
                    throw new Exception("Expected response.");
                }

            }
        }

    }

}
