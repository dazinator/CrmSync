using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using CrmSync.Dynamics;
using CrmSync.Dynamics.ComponentRegistration;
using CrmSync.Dynamics.ComponentRegistration.Enums;
using CrmSync.Dynamics.Metadata;
using CrmSync.Plugin;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using NUnit.Framework;

namespace CrmSync.Tests.SystemTests
{
    [Category("System")]
    [TestFixture]
    public class RegisterPluginSystemTests
    {

        public RegistrationInfo RegistrationInfo = null;

        public RegisterPluginSystemTests()
        {

        }

        [TestFixtureSetUp]
        public void Setup()
        {
            // Ensure custom test entity present in crm.

        }



        [Test]
        public void RegisterPlugin()
        {
            var serviceProvider = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 

            var crmOrgConnectionString = ConfigurationManager.ConnectionStrings["CrmOrganisationService"];

            var deployer = ComponentRegistrationBuilder.CreateRegistration()
                                                           .ForTheAssemblyContainingThisPlugin<CrmSyncChangeTrackerPlugin>()
                                                            .Described("Test plugin assembly")
                                                            .RunsInSandboxMode()
                                                            .LocatedInDatabase()
                                                           .HasPlugin<CrmSyncChangeTrackerPlugin>()
                                                            .ExecutesOn(SdkMessageNames.Create, "contact")
                                                            .Synchronously()
                                                            .PostOperation()
                                                            .OnlyOnServer()
                                                           .DeployTo(crmOrgConnectionString.ConnectionString);

            RegistrationInfo = deployer.Deploy();
            if (!RegistrationInfo.Success)
            {
                Assert.Fail("Registration failed..");
                //deployer.Undeploy(updateInfo);
                //Console.WriteLine("Registration was rolled back..");
            }


        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            RegistrationInfo.Undeploy();
        }

    }
}



