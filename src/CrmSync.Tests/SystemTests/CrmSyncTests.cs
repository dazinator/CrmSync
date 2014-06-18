﻿using System;
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
using Microsoft.Synchronization.Data;
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

            //Make changes on the server and client.
            // InsertNewRecordOnServer();
            InsertNewRecordOnClient();

            //Subsequent synchronization.
            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "second");

            syncStatistics = sampleSyncAgent.Synchronize();
            sampleStats.DisplayStats(syncStatistics, "third");

        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());

            UnregisterPlugin(service);
            DeleteTestEntity(service);

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
                             .DecimalAttribute(CrmSyncChangeTrackerPlugin.CreatedRowVersionAttributeName,
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
                var request = new DeleteEntityRequest();
                request.LogicalName = TestDynamicsCrmServerSyncProvider.TestEntityName;
                var response = (DeleteEntityResponse)orgService.Execute(request);
                if (response == null)
                {
                    throw new Exception("Expected response.");
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
            var valuesClause = Utility.BuildSqlValuesClause(TestDynamicsCrmServerSyncProvider.InsertColumns, valuesForInsert);
            var commandText = string.Format("INSERT INTO {0} ({1}) VALUES ({2})",
                                                  TestDynamicsCrmServerSyncProvider.TestEntityName,
                                                  string.Join(",", TestDynamicsCrmServerSyncProvider.InsertColumns.Keys),
                                                  valuesClause);

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

}