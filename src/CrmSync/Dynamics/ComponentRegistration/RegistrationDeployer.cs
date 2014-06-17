using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;

namespace CrmSync.Dynamics.ComponentRegistration
{
    public class RegistrationDeployer : IRegistrationDeployer
    {
        private readonly ComponentRegistration _Registration;
        private readonly ICrmServiceProvider _ServiceProvider;

        public ComponentRegistration Registration { get { return _Registration; } }

        public RegistrationDeployer(ComponentRegistration registration, string orgConnectionString)
            : this(registration, new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig() { OrganisationServiceConnectionString = orgConnectionString }, new CrmClientCredentialsProvider()))
        {
        }

        public RegistrationDeployer(ComponentRegistration registration)
            : this(registration, new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider()))
        {
        }

        public RegistrationDeployer(ComponentRegistration registration, ICrmServiceProvider serviceProvider)
        {
            _Registration = registration;
            _ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates the registration in CRM.
        /// </summary>
        /// <returns></returns>
        public RegistrationInfo Deploy()
        {
            var result = new RegistrationInfo(this);

            try
            {
                var pluginHelper = new PluginHelper(_ServiceProvider);
                foreach (var par in _Registration.PluginAssemblyRegistrations)
                {
                    var pa = par.PluginAssembly;
                    var pluginExists = pluginHelper.DoesPluginAssemblyExist(pa.Name);
                    if (!pluginExists.Exists)
                    {
                        // Create new plugin assembly registration.
                        var newRecordId = pluginHelper.RegisterAssembly(pa);
                        pa.PluginAssemblyId = newRecordId;
                        result.RelatedEntities.Add(pa.LogicalName, newRecordId);
                    }
                    else
                    {
                        result.RelatedEntities.Add(pa.LogicalName, pluginExists.EntityReference.Id);
                    }

                    foreach (var ptr in par.PluginTypeRegistrations)
                    {
                        var pluginTypeExists = pluginHelper.DoesPluginTypeExist(ptr.PluginType.TypeName);

                        if (!pluginTypeExists.Exists)
                        {
                            // Create new plugin assembly registration.
                            var newRecordId = pluginHelper.RegisterType(ptr.PluginType);
                            ptr.PluginType.PluginTypeId = newRecordId;
                            result.RelatedEntities.Add(ptr.PluginType.LogicalName, newRecordId);
                        }
                        else
                        {
                            ptr.PluginType.PluginTypeId = pluginExists.EntityReference.Id;
                            result.RelatedEntities.Add(ptr.PluginType.LogicalName, pluginTypeExists.EntityReference.Id);
                        }

                        // for each step
                        foreach (var ps in ptr.PluginStepRegistrations)
                        {
                            // todo: check primary and secondary entity are valid.
                            // check message name is valid.
                            var messageId = pluginHelper.GetMessageId(ps.SdkMessageName);
                            ps.SdkMessageProcessingStep.SdkMessageId = new EntityReference("sdkmessage", messageId);

                            var sdkFilterMessageId = pluginHelper.GetSdkMessageFilterId(ps.PrimaryEntityName,
                                                                                        ps.SecondaryEntityName,
                                                                                        messageId);
                            ps.SdkMessageProcessingStep.SdkMessageFilterId = new EntityReference("sdkmessagefilter", sdkFilterMessageId);

                            var newRecordId = pluginHelper.RegisterStep(ps.SdkMessageProcessingStep);
                            result.RelatedEntities.Add(ps.SdkMessageProcessingStep.LogicalName, newRecordId);

                        }
                    }
                }
                result.Success = true;
            }
            catch (Exception e)
            {
                result.Error = e;
                result.Success = false;
            }
            return result;
        }

        /// <summary>
        /// Deletes any entities related to the registration, removing the registration from CRM.
        /// </summary>
        /// <param name="regisrationInfo"></param>
        public void Undeploy(RegistrationInfo regisrationInfo)
        {
            // Ensure custom test entity removed.
            var service = new CrmServiceProvider(new ExplicitConnectionStringProviderWithFallbackToConfig(), new CrmClientCredentialsProvider());

            // clean up in reverse creation order.
            var entities = regisrationInfo.RelatedEntities.Reverse();
            DeleteEntities(service, entities);
        }

        /// <summary>
        /// Ensures test entity is deleted from CRM.
        /// </summary>
        /// <param name="serviceProvider"></param>
        private void DeleteEntities(ICrmServiceProvider serviceProvider, IEnumerable<KeyValuePair<string, Guid>> entities)
        {
            using (var orgService = (OrganizationServiceContext)serviceProvider.GetOrganisationService())
            {
                foreach (var entity in entities)
                {
                    orgService.Execute(new DeleteRequest()
                        {
                            Target = new EntityReference() { LogicalName = entity.Key, Id = entity.Value }
                        });
                    // Console.WriteLine(string.Format("Deleted entity: {0} with id : {1}", entity.Key, entity.Value));
                }
            }
        }

    }
}