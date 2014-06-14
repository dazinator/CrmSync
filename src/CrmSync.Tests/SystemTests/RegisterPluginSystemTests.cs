using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrmSync.Dynamics;
using CrmSync.Dynamics.ComponentRegistration;
using CrmSync.Dynamics.ComponentRegistration.Enums;
using CrmSync.Dynamics.Metadata;
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

        public Dictionary<string, Guid> EntitiesForCleanUp = new Dictionary<string, Guid>();

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
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                var pluginAssemblies = (from p in orgService.CreateQuery("pluginassembly") select p).ToList();
                //  var pluginTypes = (from p in orgService.CreateQuery("plugintype") select p).ToList();
                //  var sdkMessageProcessingStep = (from p in orgService.CreateQuery("sdkmessageprocessingstep") select p).ToList();
                //  var sdkMessageProcessingStepImage = (from p in orgService.CreateQuery("sdkmessageprocessingstepimage") select p).ToList();
                var assy = Assembly.GetAssembly(typeof(CrmSyncChangeTrackerPlugin));

                var registration = PluginRegistrationBuilder.CreateRegistration()
                                                                    .WithPluginAssemblyThatContainsPlugin<CrmSyncChangeTrackerPlugin>()
                                                                    .HasDescription("Test plugin")
                                                                    .RunsInIsolationMode(IsolationMode.Sandbox)
                                                                    .PlaceAssemblyInDatabase(assy)
                                                                    .Build();





                var pluginHelper = new PluginHelper(serviceProvider);
                foreach (var pluginAssembly in registration.PluginAssemblies.Values)
                {
                    var pluginExists = pluginHelper.DoesPluginAssemblyExist(pluginAssembly.Name);
                    if (!pluginExists.Exists)
                    {
                        // Create new plugin assembly registration.
                        var newRecordId = pluginHelper.RegisterAssembly(pluginAssembly);
                        EntitiesForCleanUp.Add("pluginassembly", newRecordId);
                    }
                }

            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());

            // clean up
            DeleteEntities(service, EntitiesForCleanUp);
        }

        /// <summary>
        /// Ensures test entity is deleted from CRM.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void DeleteEntities(ICrmServiceProvider serviceProvider, Dictionary<string, Guid> entities)
        {
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                foreach (var entity in entities)
                {
                    try
                    {
                        orgService.Execute(new DeleteRequest()
                            {
                                Target = new EntityReference() { LogicalName = entity.Key, Id = entity.Value }
                            });

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(string.Format("Could not delete entity: {0} with id : {1}", entity.Key,
                                                        entity.Value));
                    }
                }
            }
        }

        //private bool Register(Registration registration)
        //{

        //    try
        //    {
        //        // this.errors = new Collection<string>();
        //        var result = false;


        //        XrmPluginAssembly assembly = AssemblyHelper.GetRegistrationAssembly(registrationCollection.XrmServerDetails, registration);
        //        if (assembly.Exists)
        //        {
        //            Result = AssemblyHelper.UpdateAssembly(registrationCollection.XrmServerDetails, assembly);
        //            AssemblyHelper.AddAssemblyToSolution(registrationCollection, registration, assembly);
        //        }
        //        else
        //        {
        //            assembly.AssemblyId = AssemblyHelper.RegisterAssembly(registrationCollection.XrmServerDetails, assembly);
        //            AssemblyHelper.AddAssemblyToSolution(registrationCollection, registration, assembly);
        //            Result = true;
        //        }
        //        if (assembly.Plugins.Count != 0)
        //        {
        //            foreach (XrmPlugin plugin in assembly.Plugins)
        //            {
        //                if (plugin.Exists)
        //                {
        //                    Result = PluginHelper.UpdatePlugin(registrationCollection.XrmServerDetails, plugin);
        //                }
        //                else
        //                {
        //                    plugin.PluginId = PluginHelper.RegisterPlugin(registrationCollection.XrmServerDetails,
        //                                                                  plugin, errors);
        //                    Result = true;
        //                }
        //                if (plugin.Steps.Count != 0)
        //                {
        //                    foreach (XrmPluginStep step in plugin.Steps)
        //                    {
        //                        if (step.Exists)
        //                        {
        //                            Result = StepHelper.UpdateStep(registrationCollection.XrmServerDetails, step);
        //                            StepHelper.AddStepToSolution(registrationCollection, registration, step);
        //                        }
        //                        else
        //                        {
        //                            step.StepId = StepHelper.RegisterStep(registrationCollection.XrmServerDetails,
        //                                                                  step);
        //                            StepHelper.AddStepToSolution(registrationCollection, registration, step);
        //                            Result = true;
        //                        }
        //                        if (step.Images.Count != 0)
        //                        {
        //                            foreach (XrmPluginImage image in step.Images)
        //                            {
        //                                if (image.Exists)
        //                                {
        //                                    Result = ImageHelper.UpdateImage(
        //                                        registrationCollection.XrmServerDetails, image);
        //                                }
        //                                else
        //                                {
        //                                    image.ImageId = ImageHelper.RegisterImage(registrationCollection.XrmServerDetails,
        //                                                                  image);
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return result;
        //    }
        //    catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
        //    {
        //        throw;
        //    }



        //}






    }
}



