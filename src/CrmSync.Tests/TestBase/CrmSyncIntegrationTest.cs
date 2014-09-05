using CrmDeploy;
using CrmDeploy.Enums;
using CrmSync.Dynamics.Metadata;
using CrmSync.Plugin;
using CrmSync.Tests.SystemTests;
using CrmSync.Tests.WIP;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CrmSync.Tests
{
    public abstract class CrmSyncIntegrationTest : CrmIntegrationTest
    {

        private string _SqlCompactDatabaseConnectionString;
        public string SqlCompactDatabaseConnectionString { get { return _SqlCompactDatabaseConnectionString; } }

        private string _CrmOrgConnectionString;
        public string CrmOrgConnectionString { get { return _CrmOrgConnectionString; } }

        private string _TestEntityLogicalName;
        public string TestEntityLogicalName { get { return _TestEntityLogicalName; } }

        private EntityMetadata _TestEntityMetadata;
        public EntityMetadata TestEntityMetadata { get { return _TestEntityMetadata; } }

        public RegistrationInfo PluginRegistrationInfo { get; set; }

        protected override void SetUp()
        {
            try
            {
                _TestEntityLogicalName = TestDynamicsCrmServerSyncProvider.TestEntityName;
                EnsureCrmOrgConnectionString();
                EnsureSqlCompactDatabaseConnectionString();
                DeleteSqlCompactDatabase();
                DeprovisionTestEntity();
                ProvisionTestEntity();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

        }

        protected override void TearDown()
        {
            try
            {
                UnregisterSyncPlugin();
                DeprovisionTestEntity();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        protected void DeleteSqlCompactDatabase()
        {
            Utility.DeleteAndRecreateCompactDatabase(SqlCompactDatabaseConnectionString, false);
        }

        private void EnsureSqlCompactDatabaseConnectionString()
        {
            var sqlConnectionString = ConfigurationManager.ConnectionStrings["CrmOfflineSqlCompactDb"];
            if (sqlConnectionString != null)
            {
                var currentDir = Environment.CurrentDirectory;
                var connString = ConfigurationManager.ConnectionStrings["CrmOfflineSqlCompactDb"].ConnectionString;
                connString = connString.Replace("{CurrentDir}", currentDir);
                _SqlCompactDatabaseConnectionString = connString;
            }
            else
            {
                throw new Exception("Missing connection stirng in config file, with key: 'CrmOfflineSqlCompactDb'");
            }
        }

        private void EnsureCrmOrgConnectionString()
        {
            var orgConnectionString = ConfigurationManager.ConnectionStrings["CrmOrganisation"];
            if (orgConnectionString != null)
            {
                //var currentDir = Environment.CurrentDirectory;
                var connString = ConfigurationManager.ConnectionStrings["CrmOrganisation"].ConnectionString;
                //  connString = connString.Replace("{CurrentDir}", currentDir);
                _CrmOrgConnectionString = connString;
            }
            else
            {
                throw new Exception("Missing crm connection string in config file, with key: 'CrmOrganisation'");
            }
        }

        protected virtual void ProvisionTestEntity()
        {
            CreateTestEntity();
            RegisterSyncPlugin();
        }

        protected virtual void CreateTestEntity()
        {
            using (var orgService = (OrganizationServiceContext)this.GetCrmServiceProvider().GetOrganisationService())
            {
                try
                {
                    _TestEntityMetadata = GetTestEntityMetadata();
                }
                catch (Exception e)
                {
                    if (e.Message.ToLower().StartsWith("could not find"))
                    {
                        _TestEntityMetadata = null;
                    }
                    else
                    {
                        throw;
                    }
                }

                if (_TestEntityMetadata == null)
                {
                    var createRequest = new CreateEntityRequest();
                    var entityBuilder = EntityConstruction.ConstructEntity(TestDynamicsCrmServerSyncProvider.TestEntityName);

                    var attBuilder = entityBuilder
                          .Description("Sync Plugin Test")
                          .DisplayCollectionName("Sync Plugin Test Entities")
                          .DisplayName("Sync Plugin Test")
                          .WithAttributes()
                          .NameAttribute(TestDynamicsCrmServerSyncProvider.NameAttributeName, TestDynamicsCrmServerSyncProvider.NameAttributeName, "name", "name attribute", AttributeRequiredLevel.Recommended, 255, StringFormat.Text)
                          .StringAttribute(SyncColumnInfo.CreatedBySyncClientIdAttributeName, "created by sync client", "The sync client that created this record", AttributeRequiredLevel.Recommended, 255, StringFormat.Text)

                          .DecimalAttribute(SyncColumnInfo.CreatedRowVersionAttributeName,
                                           "CrmSync Creation Version",
                                           "The RowVersion of the record when it was created.",
                                           AttributeRequiredLevel.None, 0, null, 0)
                          .DateTimeAttribute(TestDynamicsCrmServerSyncProvider.TestDatetimeColumnName,
                                           "test date time",
                                           "The test datetime value.", AttributeRequiredLevel.None, DateTimeFormat.DateAndTime, ImeMode.Auto)
                          .DateTimeAttribute(TestDynamicsCrmServerSyncProvider.TestDateOnlyColumnName,
                                           "test date time",
                                           "The test datetime value.", AttributeRequiredLevel.None, DateTimeFormat.DateOnly, ImeMode.Auto);


                    // Loop through supported decimal precision and create a decimal attribute for each precision.
                    for (int i = DecimalAttributeMetadata.MinSupportedPrecision; i <= DecimalAttributeMetadata.MaxSupportedPrecision; i++)
                    {
                        attBuilder = attBuilder.DecimalAttribute(TestDynamicsCrmServerSyncProvider.DecimalColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture),
                            "test dec " + i.ToString(CultureInfo.InvariantCulture), "test decimal field", AttributeRequiredLevel.Recommended,
                           System.Convert.ToDecimal(DecimalAttributeMetadata.MinSupportedValue), System.Convert.ToDecimal(DecimalAttributeMetadata.MaxSupportedValue), i);
                    }

                    // Loop through supported money precision and create a money attribute for each precision.
                    for (int i = MoneyAttributeMetadata.MinSupportedPrecision; i <= MoneyAttributeMetadata.MaxSupportedPrecision; i++)
                    {
                        attBuilder = attBuilder.MoneyAttribute(TestDynamicsCrmServerSyncProvider.MoneyColumnNamePrefix + i.ToString(CultureInfo.InvariantCulture),
                            "test money " + i.ToString(CultureInfo.InvariantCulture), "test money field", AttributeRequiredLevel.Recommended,
                           MoneyAttributeMetadata.MinSupportedValue, MoneyAttributeMetadata.MaxSupportedValue, i, 0);
                    }

                    attBuilder = attBuilder.BooleanAttribute(TestDynamicsCrmServerSyncProvider.BoolColumnName, "test bool",
                                                "test bool field", AttributeRequiredLevel.Recommended, "Yes", 1, "No", 2);

                    //// Add in all possible integer formats.
                    //var enumVals = Enum.GetValues(typeof(IntegerFormat));
                    //foreach (var enumVal in enumVals)
                    //{
                    //    IntegerFormat format = (IntegerFormat)enumVal;
                    //    string formatName = format.ToString();

                    //    attBuilder = attBuilder.IntAttribute(TestDynamicsCrmServerSyncProvider.IntColumnName + formatName, "test int" + formatName,
                    //                        "test int field", AttributeRequiredLevel.Recommended, format, 0, int.MaxValue);

                    //}

                    // TODO experiment with other formats of memo although not sure they really make sense..
                    attBuilder.MemoAttribute(TestDynamicsCrmServerSyncProvider.MemoColumnName, "test memo",
                                             "test memo field", AttributeRequiredLevel.Recommended, 255,
                                             StringFormat.TextArea);


                    // TODO experiment with other formats of memo although not sure they really make sense..
                    var options = new Dictionary<string, int>();
                    for (int i = TestDynamicsCrmServerSyncProvider.PicklistColumnMinValue; i <= TestDynamicsCrmServerSyncProvider.PicklistColumnMaxValue; i++)
                    {
                        string labelText = "testoption" + i;
                        var optionVal = i;
                        options.Add(labelText, optionVal);
                    }

                    //todo what about global?
                    //todo what about other OptionSetTypes?
                    attBuilder.PicklistAttribute(TestDynamicsCrmServerSyncProvider.PicklistColumnName, "test picklist",
                                             "test picklist field", AttributeRequiredLevel.Recommended, false, OptionSetType.Picklist, options);


                    var entityMetadata = entityBuilder.Build();
                    createRequest.Entity = entityMetadata;


                    //  createRequest.HasActivities = false;
                    //  createRequest.HasNotes = false;
                    createRequest.PrimaryAttribute = entityBuilder.GetNameAttribute();
                    //  createRequest.SolutionUniqueName =                
                    try
                    {

                        var createResponse = (CreateEntityResponse)orgService.Execute(createRequest);
                        foreach (var att in entityBuilder.AttributeBuilder.Attributes.Where(a => a.SchemaName != TestDynamicsCrmServerSyncProvider.NameAttributeName))
                        {
                            try
                            {
                                var createAttributeRequest = new CreateAttributeRequest
                                {
                                    EntityName = entityBuilder.Entity.LogicalName,
                                    Attribute = att
                                };
                                var createAttResponse = (CreateAttributeResponse)orgService.Execute(createAttributeRequest);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Could not create attribute: " + att.LogicalName + ", because " + ex.Message);
                                // throw;
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        throw;
                    }

                    this._TestEntityMetadata = GetTestEntityMetadata();
                }
            }
        }

        private EntityMetadata GetTestEntityMetadata()
        {
            using (var orgService = (OrganizationServiceContext)this.GetCrmServiceProvider().GetOrganisationService())
            {
                // Check for test entity - if it doesn't exist then create it.
                var request = new RetrieveEntityRequest();
                request.RetrieveAsIfPublished = true;
                request.LogicalName = TestEntityLogicalName;

                RetrieveEntityResponse response = null;
                response = (RetrieveEntityResponse)orgService.Execute(request);
                return response.EntityMetadata;
            }
        }

        protected virtual void DeprovisionTestEntity()
        {
            RemoveTestEntity();
            UnregisterSyncPlugin();
        }

        protected virtual void RemoveTestEntity()
        {
            using (var orgService = (OrganizationServiceContext)this.GetCrmServiceProvider().GetOrganisationService())
            {
                // Check for test entity - if it doesn't exist then create it.

                var retrieveEntity = new RetrieveEntityRequest();
                retrieveEntity.RetrieveAsIfPublished = true;
                retrieveEntity.LogicalName = TestDynamicsCrmServerSyncProvider.TestEntityName;
                RetrieveEntityResponse retrieveEntityResponse = null;
                try
                {
                    var entity = GetTestEntityMetadata();
                }
                catch (Exception e)
                {
                    if (e.Message.ToLower().StartsWith("could not find"))
                    {
                        // this means no need to remove the entity as it doesn't exist.
                        return;
                    }
                    else
                    {
                        // This means some other error happened.
                        // Write the error, but proceed so we can atleast still attempt to remove the entity.
                        Console.WriteLine("error checking whether entity exists, but will still try and remove it. Message: " + e.Message);
                    }

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
                    Console.WriteLine("Error deleting test entity metadata. " + e.Message);

                    RetrieveDependenciesForDeleteRequest req = new RetrieveDependenciesForDeleteRequest();
                    req.ComponentType = (int)SolutionComponentType.Entity;
                    //use the metadata browser or a retrieveentity request to get the MetadataId for the entity
                    req.ObjectId = retrieveEntityResponse.EntityMetadata.MetadataId.Value;
                    var dependenciesResponse = (RetrieveDependenciesForDeleteResponse)orgService.Execute(req);

                    foreach (Entity item in dependenciesResponse.EntityCollection.Entities)
                    {
                        Console.WriteLine("Could not remove the entity because of a dependency: " + item.LogicalName + " id: " + item.Id);
                    }

                }


            }
        }

        private void RegisterSyncPlugin()
        {


            var orgConnectionString = this._CrmOrgConnectionString;
            var deployer = DeploymentBuilder.CreateDeployment()
                                                          .ForTheAssemblyContainingThisPlugin<CrmSyncChangeTrackerPlugin>("Test plugin assembly")
                                                           .RunsInSandboxMode()
                                                           .RegisterInDatabase()
                                                          .HasPlugin<CrmSyncChangeTrackerPlugin>()
                                                           .WhichExecutesOn(SdkMessageNames.Create, TestDynamicsCrmServerSyncProvider.TestEntityName)
                                                           .Synchronously()
                                                           .PostOperation()
                                                           .OnlyOnCrmServer()
                                                          .DeployTo(orgConnectionString);

            PluginRegistrationInfo = deployer.Deploy();
            if (!PluginRegistrationInfo.Success)
            {
                Console.WriteLine("Plugin registration failed.. rolling back..");
                try
                {
                    PluginRegistrationInfo.Undeploy();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Rollback failed. " + e.Message);
                }
                Assert.Fail("Could not register plugin.");

            }
        }

        private void UnregisterSyncPlugin()
        {
            if (PluginRegistrationInfo != null)
            {
                PluginRegistrationInfo.Undeploy();
            }
        }



    }
}
