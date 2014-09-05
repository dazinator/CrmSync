using System.Configuration;
using CrmDeploy;
using CrmDeploy.Enums;
using CrmSync.Dynamics;
using CrmSync.Plugin;
using NUnit.Framework;

namespace CrmSync.Tests.SystemTests
{

    [Category("Plugin")]
    [TestFixture]
    public class RegisterPluginSystemTests : CrmIntegrationTest
    {
        public RegistrationInfo RegistrationInfo = null;

        protected override void SetUp()
        {
        }

        public RegisterPluginSystemTests()
        {

        }

        [Test]
        public void Can_Register_Crm_Plugin()
        {
            var serviceProvider = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 

            var crmOrgConnectionString = ConfigurationManager.ConnectionStrings["CrmOrganisation"];

            var deployer = DeploymentBuilder.CreateDeployment()
                                                           .ForTheAssemblyContainingThisPlugin<CrmSyncChangeTrackerPlugin>("Test plugin assembly")
                                                           .RunsInSandboxMode()
                                                           .RegisterInDatabase()
                                                           .HasPlugin<CrmSyncChangeTrackerPlugin>()
                                                            .WhichExecutesOn(SdkMessageNames.Create, "contact")
                                                            .Synchronously()
                                                            .PostOperation()
                                                            .OnlyOnCrmServer()
                                                           .DeployTo(crmOrgConnectionString.ConnectionString);

            RegistrationInfo = deployer.Deploy();
            if (!RegistrationInfo.Success)
            {
                Assert.Fail("Registration failed..");
                //deployer.Undeploy(updateInfo);
                //Console.WriteLine("Registration was rolled back..");
            }


        }

        protected override void TearDown()
        {
            RegistrationInfo.Undeploy();
        }
    }
}



