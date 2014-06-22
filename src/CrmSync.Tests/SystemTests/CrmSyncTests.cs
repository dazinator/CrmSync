using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Globalization;
using System.Linq;
using System.Text;
using CrmAdo;
using CrmDeploy;
using CrmDeploy.Enums;
using CrmSync.Dynamics;
using CrmSync.Dynamics.Metadata;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Synchronization.Data;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;
using CrmSync.Plugin;

namespace CrmSync.Tests.SystemTests
{
    [Category("System")]
    [Category("Sync")]
    [TestFixture]
    public class CrmSyncTests
    {


        public string SqlCompactDatabaseConnectionString;

        public string CrmConnectionString;

        public CrmSyncTests()
        {

        }

        public RegistrationInfo PluginRegistrationInfo { get; set; }

        [TestFixtureSetUp]
        public void Setup()
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
            Console.WriteLine(dataDirectory);

            if (dataDirectory == null)
            {
                var appBase = AppDomain.CurrentDomain.GetData("APPBASE");
                AppDomain.CurrentDomain.SetData("DataDirectory", appBase);
                Console.WriteLine(appBase);
            }

            DeleteSqlCompactDatabase();

            // LoadColumnInfo();

            // Ensure custom test entity present in crm.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            DeleteTestEntity(service);
            CrmConnectionString = service.ConnectionProvider.OrganisationServiceConnectionString;
            CreateTestEntity(service);

            // Ensure Plugin Registered in CRM.
            RegisterPlugin(service);
        }

        [Test]
        public void Can_Sync_Incremental_Create()
        {
            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
            Console.WriteLine(dataDirectory);

            if (dataDirectory == null)
            {
                Assert.Fail("No data directory.");
            }

            var sampleStats = new SampleStats();

            var sampleSyncAgent = new TestDynamicsCrmSyncAgent(SqlCompactDatabaseConnectionString, CrmConnectionString);
            SyncStatistics syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "initial");

            // get number of existing records.
            // assert that the client only has one record and that the server only has 1 record.
            int existingCount = 0;
            using (var clientConn = new SqlCeConnection(SqlCompactDatabaseConnectionString))
            {
                clientConn.Open();
                using (var sqlCeCommand = clientConn.CreateCommand())
                {
                    sqlCeCommand.CommandText = string.Format("SELECT COUNT({0}) FROM {1}", TestDynamicsCrmServerSyncProvider.IdAttributeName, TestDynamicsCrmServerSyncProvider.TestEntityName);
                    existingCount = (int)sqlCeCommand.ExecuteScalar();
                    // Assert.That(rowCount, Is.EqualTo(1), string.Format("Only 1 record was synchronised however {0} records ended up in the client database!", rowCount));
                }
                clientConn.Close();
            }


            //Make changes on the server and client.
            // InsertNewRecordOnServer();
            InsertNewRecordOnClient();

            //Subsequent synchronization.
            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "second");

            // Verfiy new record added on server.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            using (var orgService = service.GetOrganisationService() as OrganizationServiceContext)
            {
                var entity = (from a in orgService.CreateQuery(TestDynamicsCrmServerSyncProvider.TestEntityName) orderby a["createdon"] descending select a).FirstOrDefault();
                Assert.That(entity, Is.Not.Null);
                var clientId = entity.Attributes[SyncColumnInfo.CreatedBySyncClientIdAttributeName];
                Assert.That(clientId, Is.EqualTo(SelectIncrementalCreatesCommand.SyncClientId), "A record was inserted during synchronisation, however it did not have a client id set.");

            }

            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "third");

            // assert that the client only has one record and that the server only has 1 record.
            using (var clientConn = new SqlCeConnection(SqlCompactDatabaseConnectionString))
            {
                clientConn.Open();
                using (var sqlCeCommand = clientConn.CreateCommand())
                {
                    sqlCeCommand.CommandText = string.Format("SELECT COUNT({0}) FROM {1}", TestDynamicsCrmServerSyncProvider.IdAttributeName, TestDynamicsCrmServerSyncProvider.TestEntityName);
                    var rowCount = (int)sqlCeCommand.ExecuteScalar();
                    Assert.That(rowCount, Is.EqualTo(existingCount + 1), string.Format("Only 1 new record was created, however after a few synchronisations, {0} new records ended up in the client database!", rowCount - existingCount));
                }
                clientConn.Close();
            }


        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            Exception ex = null;
            try
            {
                UnregisterPlugin(service);
            }
            catch (Exception e)
            {
                ex = e;
            }

            try
            {
                DeleteTestEntity(service);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, ex);
            }


        }

        #region Utility Methods

        private void RegisterPlugin(CrmServiceProvider serviceProvider)
        {
            var orgConnectionString = ConfigurationManager.ConnectionStrings["CrmOrganisationService"];

            var deployer = DeploymentBuilder.CreateDeployment()
                                                           .ForTheAssemblyContainingThisPlugin<CrmSyncChangeTrackerPlugin>("Test plugin assembly")
                                                            .RunsInSandboxMode()
                                                            .RegisterInDatabase()
                                                           .HasPlugin<CrmSyncChangeTrackerPlugin>()
                                                            .WhichExecutesOn(SdkMessageNames.Create, TestDynamicsCrmServerSyncProvider.TestEntityName)
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
                request.LogicalName = TestDynamicsCrmServerSyncProvider.TestEntityName;
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


                    var entityBuilder = EntityConstruction.ConstructEntity(TestDynamicsCrmServerSyncProvider.TestEntityName);
                    createRequest.Entity = entityBuilder
                             .Description("Sync Plugin Test")
                             .DisplayCollectionName("Sync Plugin Test Entities")
                             .DisplayName("Sync Plugin Test")
                             .WithAttributes()
                             .StringAttribute(TestDynamicsCrmServerSyncProvider.NameAttributeName, "name", "name attribute", AttributeRequiredLevel.Recommended, 255, StringFormat.Text)
                             .StringAttribute(SyncColumnInfo.CreatedBySyncClientIdAttributeName, "created by sync client", "The sync client that created this record", AttributeRequiredLevel.Recommended, 255, StringFormat.Text)

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
                        foreach (var att in entityBuilder.AttributeBuilder.Attributes.Where(a => a.SchemaName != TestDynamicsCrmServerSyncProvider.NameAttributeName))
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

                var retrieveEntity = new RetrieveEntityRequest();
                retrieveEntity.RetrieveAsIfPublished = true;
                retrieveEntity.LogicalName = TestDynamicsCrmServerSyncProvider.TestEntityName;
                RetrieveEntityResponse retrieveEntityResponse = null;
                try
                {
                    retrieveEntityResponse = (RetrieveEntityResponse)orgService.Execute(retrieveEntity);
                }
                catch (Exception e)
                {
                    return;
                }

                var request = new DeleteEntityRequest();
                request.LogicalName = TestDynamicsCrmServerSyncProvider.TestEntityName;
                try
                {
                    var response = (DeleteEntityResponse)orgService.Execute(request);
                    if (response == null)
                    {
                        throw new Exception("Expected response.");
                    }
                }
                catch (Exception e)
                {


                    RetrieveDependenciesForDeleteRequest req = new RetrieveDependenciesForDeleteRequest();
                    req.ComponentType = (int)SolutionComponentType.Entity;
                    //use the metadata browser or a retrieveentity request to get the MetadataId for the entity
                    req.ObjectId = retrieveEntityResponse.EntityMetadata.MetadataId.Value;
                    var dependenciesResponse = (RetrieveDependenciesForDeleteResponse)orgService.Execute(req);

                    foreach (Entity item in dependenciesResponse.EntityCollection.Entities)
                    {
                        Console.WriteLine("Could not delte entity because dependency: " + item.LogicalName + " id: " + item.Id);
                    }

                }


            }
        }

        private void DeleteSqlCompactDatabase()
        {
            var sqlConnectionString = ConfigurationManager.ConnectionStrings["CrmOfflineSqlCompactDb"];
            if (sqlConnectionString != null)
            {
                SqlCompactDatabaseConnectionString = ConfigurationManager.ConnectionStrings["CrmOfflineSqlCompactDb"].ConnectionString;
            }
            else
            {
                throw new Exception("Missing connection stirng in config file, with key: 'CrmOfflineSqlCompactDb'");
            }

            Utility.DeleteAndRecreateCompactDatabase(SqlCompactDatabaseConnectionString, false);
        }

        private void InsertNewRecordOnClient()
        {
            var valuesForInsert = new Dictionary<string, string>();
            var valuesClause = Utility.BuildSqlValuesClause(TestDynamicsCrmServerSyncProvider.ClientInsertColumns, valuesForInsert);
            var sqlColumns = string.Join(",",
                                         TestDynamicsCrmServerSyncProvider.ClientInsertColumns.Select(
                                             s => s.AttributeName));
            var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                                  TestDynamicsCrmServerSyncProvider.TestEntityName, sqlColumns, valuesClause);

            int rowCount = 0;
            using (var clientConn = new SqlCeConnection(SqlCompactDatabaseConnectionString))
            {
                clientConn.Open();
                using (var sqlCeCommand = clientConn.CreateCommand())
                {
                    sqlCeCommand.CommandText = commandText;
                    rowCount = sqlCeCommand.ExecuteNonQuery();
                }
                clientConn.Close();
            }
            Console.WriteLine("{0} Rows inserted at the client", rowCount);
        }

        #endregion

    }


    public enum SolutionComponentType
    {
        Entity = 1,
        Attribute = 2,
        Relationship = 3,
        AttributePicklistValue = 4,
        AttributeLookupValue = 5,
        ViewAttribute = 6,
        LocalizedLabel = 7,
        RelationshipExtraCondition = 8,
        OptionSet = 9,
        EntityRelationship = 10,
        EntityRelationshipRole = 11,
        EntityRelationshipRelationships = 12,
        ManagedProperty = 13,
        Role = 20,
        RolePrivilege = 21,
        DisplayString = 22,
        DisplayStringMap = 23,
        Form = 24,
        Organization = 25,
        SavedQuery = 26,
        Workflow = 29,
        Report = 31,
        ReportEntity = 32,
        ReportCategory = 33,
        ReportVisibility = 34,
        Attachment = 35,
        EmailTemplate = 36,
        ContractTemplate = 37,
        KBArticleTemplate = 38,
        MailMergeTemplate = 39,
        DuplicateRule = 44,
        DuplicateRuleCondition = 45,
        EntityMap = 46,
        AttributeMap = 47,
        RibbonCommand = 48,
        RibbonContextGroup = 49,
        RibbonCustomization = 50,
        RibbonRule = 52,
        RibbonTabToCommandMap = 53,
        RibbonDiff = 55,
        SavedQueryVisualization = 59,
        SystemForm = 60,
        WebResource = 61,
        SiteMap = 62,
        ConnectionRole = 63,
        FieldSecurityProfile = 70,
        FieldPermission = 71,
        PluginType = 90,
        PluginAssembly = 91,
        SDKMessageProcessingStep = 92,
        SDKMessageProcessingStepImage = 93,
        ServiceEndpoint = 95
    }
}
