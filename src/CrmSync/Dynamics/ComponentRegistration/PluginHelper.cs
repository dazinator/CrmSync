using System;
using System.Reflection;
using System.ServiceModel;
using CrmSync.Dynamics.Entities;
using Microsoft.Xrm.Client.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace CrmSync.Dynamics.ComponentRegistration
{
    public class PluginHelper
    {
        private ICrmServiceProvider _ServiceProvider;

        public PluginHelper(ICrmServiceProvider serviceProvider)
        {
            _ServiceProvider = serviceProvider;
        }

        public EntityExists DoesPluginAssemblyExist(AssemblyName assemblyName)
        {
            using (var orgService = (OrganizationServiceContext)_ServiceProvider.GetOrganisationService())
            {
                var query = new QueryByAttribute(PluginAssembly.EntityLogicalName);
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("name");
                query.Values.AddRange(assemblyName.Name);
                var results = orgService.RetrieveMultiple(query);
                if (results.Entities != null && results.Entities.Count > 0)
                {
                    var reference = new EntityReference(PluginAssembly.EntityLogicalName, results.Entities[0].Id);
                    return EntityExists.Yes(reference);
                }
                else
                {
                    return EntityExists.No();
                }
            }
        }

        public EntityExists DoesPluginAssemblyExist(string pluginName)
        {
            using (var orgService = (OrganizationServiceContext)_ServiceProvider.GetOrganisationService())
            {
                var query = new QueryByAttribute(PluginAssembly.EntityLogicalName);
                query.ColumnSet = new ColumnSet(true);
                query.Attributes.AddRange("name");
                query.Values.AddRange(pluginName);
                var results = orgService.RetrieveMultiple(query);
                if (results.Entities != null && results.Entities.Count > 0)
                {
                    var reference = new EntityReference(PluginAssembly.EntityLogicalName, results.Entities[0].Id);
                    return EntityExists.Yes(reference);
                }
                else
                {
                    return EntityExists.No();
                }
            }
        }

        public Guid RegisterAssembly(PluginAssembly pluginAssembly)
        {
            try
            {
                using (var orgService = (OrganizationServiceContext)_ServiceProvider.GetOrganisationService())
                {
                    var response = (CreateResponse)orgService.Execute(new CreateRequest() { Target = pluginAssembly });
                    return response.id;
                }
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
                throw;
            }
        }
    }
}