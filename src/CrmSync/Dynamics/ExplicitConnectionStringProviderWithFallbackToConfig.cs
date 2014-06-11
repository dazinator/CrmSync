using System;
using System.Configuration;
using Microsoft.Xrm.Client;

namespace CrmSync.Dynamics
{

    /// <summary>
    /// Single Responsibility: This class is responsible for providing "Crm Connnection" information for the Crm services.
    /// This implementation uses explicitly provided connection string information (properties must be set), or falls back to using the AppConfig if
    /// no connection information is explicitly provided. 
    /// </summary>
    public class ExplicitConnectionStringProviderWithFallbackToConfig : CrmConnectionProvider
    {

        public const string DeploymentConnectionStringKey = "CrmDeploymentService";
        public const string DiscoveryConnectionStringKey = "CrmDiscoveryService";
        public const string OrgConnectionStringKey = "CrmOrganisationService";

        public override CrmConnection GetOrganisationServiceConnection()
        {
            var conn = string.IsNullOrEmpty(OrganisationServiceConnectionString)
                           ? CreateConnectionFromConnectionStringInConfigFile(OrgConnectionStringKey)
                           : CreateConnectionFromConnectionString(OrganisationServiceConnectionString);

            return conn;
        }

        public override CrmConnection GetDeploymentServiceConnection()
        {
            var conn = string.IsNullOrEmpty(DeploymentServiceConnectionString)
                           ? CreateConnectionFromConnectionStringInConfigFile(DeploymentConnectionStringKey)
                           : CreateConnectionFromConnectionString(DeploymentServiceConnectionString);

            return conn;
        }

        public override CrmConnection GetDiscoveryServiceConnection()
        {
            var conn = string.IsNullOrEmpty(DiscoveryServiceConnectionString)
                           ? CreateConnectionFromConnectionStringInConfigFile(DiscoveryConnectionStringKey)
                           : CreateConnectionFromConnectionString(DiscoveryServiceConnectionString);

            return conn;
        }
     
        protected CrmConnection CreateConnectionFromConnectionStringInConfigFile(string key)
        {
            var connStringSetting = ConfigurationManager.ConnectionStrings[key];
            if (connStringSetting == null)
            {
                throw new ArgumentException("Connection string for a required Crm service was not found in the connectionStrings section of your config file. The missing connection string name is:" + key);
            }
            return CreateConnectionFromConnectionString(connStringSetting.ConnectionString);
        }

        protected CrmConnection CreateConnectionFromConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.");

            }
            return CrmConnection.Parse(connectionString);
        }

    }
}