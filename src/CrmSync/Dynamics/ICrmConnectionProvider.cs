using Microsoft.Xrm.Client;

namespace CrmSync.Dynamics
{
    /// <summary>
    /// Classes that supply CrmConnection's used for creating instances of Crm services will implement this interface.
    /// </summary>
    public interface ICrmConnectionProvider
    {
        CrmConnection GetOrganisationServiceConnection();
        CrmConnection GetDeploymentServiceConnection();
        CrmConnection GetDiscoveryServiceConnection();


        string OrganisationServiceConnectionString { get; set; }
        string DeploymentServiceConnectionString { get; set; }
        string DiscoveryServiceConnectionString { get; set; }
    }
}