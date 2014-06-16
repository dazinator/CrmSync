using System;
using System.Linq;
using CrmSync.Dynamics;
using CrmSync.Dynamics.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;

namespace CrmSync.Tests.SystemTests
{
    [Category("System")]
    [TestFixture]
    public class SdkMessageTests
    {
        //  public const string TestEntityName = "crmsync_testpluginentity";

        public SdkMessageTests()
        {

        }

        [Test]
        public void GetSdkMessages()
        {
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 
            using (var orgService = (OrganizationServiceContext)service.GetOrganisationService())
            {
                //var pluginAssemblies = (from p in orgService.CreateQuery("pluginassembly") select p).ToList();
                //var pluginTypes = (from p in orgService.CreateQuery("plugintype") select p).ToList();
                //var sdkMessageProcessingStep = (from p in orgService.CreateQuery("sdkmessageprocessingstep") select p).ToList();
                //var sdkMessageProcessingStepImage = (from p in orgService.CreateQuery("sdkmessageprocessingstepimage") select p).ToList();
                var sdkMessages = (from p in orgService.CreateQuery("sdkmessage") select p["name"]).ToList();
                foreach (var sdkMessage in sdkMessages)
                {
                    Console.WriteLine(sdkMessage);
                }



            }
        }

        [Test]
        public void GetPluginTypeId()
        {
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 
            using (var orgService = (OrganizationServiceContext)service.GetOrganisationService())
            {
                //var pluginAssemblies = (from p in orgService.CreateQuery("pluginassembly") select p).ToList();
                //var pluginTypes = (from p in orgService.CreateQuery("plugintype") select p).ToList();
                //var sdkMessageProcessingStep = (from p in orgService.CreateQuery("sdkmessageprocessingstep") select p).ToList();
                //var sdkMessageProcessingStepImage = (from p in orgService.CreateQuery("sdkmessageprocessingstepimage") select p).ToList();

                //sdkmessagefilterid 
                //plugintypeid 
                var sdkMessageProcessingSteps = (from p in orgService.CreateQuery("sdkmessageprocessingstep") where p["plugintypeid"] != null select p["plugintypeid"]).ToList();

                foreach (var sdkMessageProcessingStep in sdkMessageProcessingSteps)
                {
                    var ef = (EntityReference)sdkMessageProcessingStep;
                    Console.Write(ef.Id);
                    Console.Write(",");
                    Console.Write(ef.LogicalName);
                    Console.Write(",");
                    Console.Write(ef.Name);
                    Console.WriteLine();
                }
                var sdkMessages = (from s in orgService.CreateQuery("sdkmessageprocessingstep")
                                   join f in orgService.CreateQuery("sdkmessagefilter") on s["plugintypeid"] equals f["sdkmessagefilterid"]
                                   select f).ToList();
                foreach (var sdkMessage in sdkMessages)
                {

                    foreach (var ar in sdkMessage.Attributes)
                    {
                        Console.Write(ar.Value);
                    }
                    Console.WriteLine();

                }



            }
        }


        [Test]
        public void GetMessaheFilterId()
        {
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());
            //PluginAssembly, PluginType, SdkMessageProcessingStep, and SdkMessageProcessingStepImage. 
            using (var orgService = (OrganizationServiceContext)service.GetOrganisationService())
            {
                //var pluginAssemblies = (from p in orgService.CreateQuery("pluginassembly") select p).ToList();
                //var pluginTypes = (from p in orgService.CreateQuery("plugintype") select p).ToList();
                //var sdkMessageProcessingStep = (from p in orgService.CreateQuery("sdkmessageprocessingstep") select p).ToList();
                //var sdkMessageProcessingStepImage = (from p in orgService.CreateQuery("sdkmessageprocessingstepimage") select p).ToList();

                //sdkmessagefilterid 
                //plugintypeid 
                //  var sdkMessageProcessingSteps = (from p in orgService.CreateQuery("sdkmessageprocessingstep") where p["sdkmessagefilterid"] != null select p["sdkmessagefilterid"]).ToList();

                //foreach (var sdkMessageProcessingStep in sdkMessageProcessingSteps)
                //{
                //    var ef = (EntityReference)sdkMessageProcessingStep;
                //    Console.Write(ef.Id);
                //    Console.Write(",");
                //    Console.Write(ef.LogicalName);
                //    Console.Write(",");
                //    Console.Write(ef.Name);
                //    Console.WriteLine();
                //}
                var sdkMessages = (from s in orgService.CreateQuery("sdkmessageprocessingstep")
                                   join f in orgService.CreateQuery("sdkmessagefilter") on s["sdkmessagefilterid"] equals f["sdkmessagefilterid"]
                                   select f).ToList();

                if (sdkMessages.Any())
                {
                    var columnsEntity = sdkMessages[0];
                    foreach (var att in columnsEntity.Attributes)
                    {
                        Console.Write(att.Key);
                        Console.Write(",");
                    }
                    Console.WriteLine();
                }
                foreach (var sdkMessage in sdkMessages)
                {
                    foreach (var ar in sdkMessage.Attributes)
                    {
                        var value = ar.Value as EntityReference;
                        if (value != null)
                        {
                            Console.Write(value.Id);
                        }
                        else
                        {
                            var ovalue = ar.Value as OptionSetValue;
                            if (ovalue != null)
                            {
                                Console.Write(ovalue.Value);
                            }
                            else
                            {
                                Console.Write(ar.Value);
                            }
                        }
                        Console.Write(",");
                    }
                    Console.WriteLine();
                }
            }
        }




    }

}
