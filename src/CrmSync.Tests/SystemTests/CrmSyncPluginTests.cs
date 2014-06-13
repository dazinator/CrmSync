using System;
using System.Linq;
using CrmSync.Dynamics;
using CrmSync.Dynamics.Metadata;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;

namespace CrmSync.Tests.SystemTests
{
    [Category("System")]
    [TestFixture]
    public class CrmSyncPluginSystemTests
    {
        public const string TestEntityName = "crmsync_testpluginentity";

        public CrmSyncPluginSystemTests()
        {

        }

        [TestFixtureSetUp]
        public void Setup()
        {
            // Ensure custom test entity present in crm.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            CreateTestEntity(service);

            // Ensure Plugin Registered in CRM.
            RegisterPlugin(service);

        }

        private void RegisterPlugin(CrmServiceProvider serviceProvider)
        {
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                var pluginAssemblies = (from p in orgService.CreateQuery("pluginassembly") select p).ToList();
                var pluginTypes = (from p in orgService.CreateQuery("plugintype") select p).ToList();
                var sdkMessageProcessingStep = (from p in orgService.CreateQuery("sdkmessageprocessingstep") select p).ToList();
                var sdkMessageProcessingStepImage = (from p in orgService.CreateQuery("sdkmessageprocessingstepimage") select p).ToList();

            }
        }


        [Test]
        public void Should_Set_Custom_Row_Version_Attribute()
        {


        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            DeleteTestEntity(service);

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
                    string nameAttributeName = "crmsync_testpluginentityname";

                    var entityBuilder = EntityConstruction.ConstructEntity(TestEntityName);
                    createRequest.Entity = entityBuilder
                             .Description("Sync Plugin Test")
                             .DisplayCollectionName("Sync Plugin Test Entities")
                             .DisplayName("Sync Plugin Test")
                             .WithAttributes()
                             .StringAttribute(nameAttributeName, "name", "name attribute", AttributeRequiredLevel.Recommended, 255, StringFormat.Text)
                             .DecimalAttribute(CrmSyncChangeTrackerPlugin.CreatedRowVersionAttributeName,
                                              "CrmSync Creation Version",
                                              "The RowVersion of the record when it was created.",
                                              AttributeRequiredLevel.None,0,null,0)
                             .MetaDataBuilder.Build();

                  //  createRequest.HasActivities = false;
                  //  createRequest.HasNotes = false;
                    createRequest.PrimaryAttribute = (StringAttributeMetadata)entityBuilder.AttributeBuilder.Attributes[0];
                  //  createRequest.SolutionUniqueName =

                    try
                    {
                        var createResponse = (CreateEntityResponse)orgService.Execute(createRequest);
                        foreach (var att in entityBuilder.AttributeBuilder.Attributes.Where(a => a.SchemaName != nameAttributeName))
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
