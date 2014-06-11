using Microsoft.Xrm.Client;

namespace CrmSync.Dynamics
{

    /// <summary>
    /// Single Responsibility: This is the abstract / base class for a provider that can supply CrmConnections for Crm services.
    /// </summary>
    public abstract class CrmConnectionProvider : ICrmConnectionProvider
    {
        public abstract CrmConnection GetOrganisationServiceConnection();
        public abstract CrmConnection GetDeploymentServiceConnection();
        public abstract CrmConnection GetDiscoveryServiceConnection();
       
        public string OrganisationServiceConnectionString { get; set; }
        public string DeploymentServiceConnectionString { get; set; }
        public string DiscoveryServiceConnectionString { get; set; }
    }
}