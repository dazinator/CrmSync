using System;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Plugin
{
    public abstract class BasePlugin : IPlugin
    {
        private ITracingService TracingService { get; set; }

        protected IPluginExecutionContext Context { get; set; }

        protected IOrganizationServiceFactory OrganisationServiceFactory { get; set; }

        protected IServiceProvider ServiceProvider { get; set; }

        protected virtual IOrganizationService GetOrganisationService()
        {
            return GetOrganisationService(Context.UserId);
        }

        protected virtual IOrganizationService GetOrganisationService(Guid userId)
        {
            if (OrganisationServiceFactory != null)
            {
                return OrganisationServiceFactory.CreateOrganizationService(userId);
            }
            throw new InvalidOperationException("Organisation Service Factory is not initialised.");
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                ServiceProvider = serviceProvider;
                LoadServices(serviceProvider);
                this.Execute();
            }
            catch (InvalidPluginExecutionException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        protected virtual void LoadServices(IServiceProvider serviceProvider)
        {
            Context = (IPluginExecutionContext)ServiceProvider.GetService(typeof(IPluginExecutionContext));
            OrganisationServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            TracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
        }
        
        protected abstract void Execute();

        protected void Trace(string format, params object[] args)
        {
            TracingService.Trace(format, args);
        }

        protected virtual Entity EnsureTargetEntity()
        {
            var targetEntity = GetTargetEntity();
            if (targetEntity == null)
            {
                Fail("Could not get Target Entity");
            }
            return targetEntity;
        }

        protected virtual void EnsureTransaction()
        {
            if (!Context.IsInTransaction)
            {
                Fail("The plugin detected that it was not running within a database transaction. The plugin requires a database transaction.");
                return;
            }
        }

        protected virtual Entity GetTargetEntity()
        {
            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                var entity = (Entity)Context.InputParameters["Target"];
                return entity;

            }
            return null;
        }

        protected virtual EntityReference GetTargetEntityReference()
        {
            // The InputParameters collection contains all the data passed in the message request.
            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is EntityReference)
            {
                // Obtain the target entity from the input parameters.
                EntityReference entity = (EntityReference)Context.InputParameters["Target"];
                return entity;
            }
            return null;
        }

        protected void Fail(string message)
        {
            throw new InvalidPluginExecutionException(message);
        }

        public virtual string PluginName()
        {
            return this.GetType().Name;
        }
      

    }
}