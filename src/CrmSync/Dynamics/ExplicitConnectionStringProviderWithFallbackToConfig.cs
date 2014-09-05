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

        public const string DeploymentConnectionStringKey = "CrmDeployment";
        public const string DiscoveryConnectionStringKey = "CrmDiscovery";
        public const string OrgConnectionStringKey = "CrmOrganisation";

        public override CrmConnection GetOrganisationServiceConnection()
        {
            ConnectionStringType connType = ConnectionStringType.OrgService;
            var conn = string.IsNullOrEmpty(OrganisationServiceConnectionString)
                           ? CreateConnectionFromConnectionStringInConfigFile(OrgConnectionStringKey, connType)
                           : CreateConnectionFromConnectionString(OrganisationServiceConnectionString, connType);

            return conn;
        }

        public override CrmConnection GetDeploymentServiceConnection()
        {
            ConnectionStringType connType = ConnectionStringType.DeploymentService;
            var conn = string.IsNullOrEmpty(DeploymentServiceConnectionString)
                           ? CreateConnectionFromConnectionStringInConfigFile(DeploymentConnectionStringKey, connType)
                           : CreateConnectionFromConnectionString(DeploymentServiceConnectionString, connType);

            return conn;
        }

        public override CrmConnection GetDiscoveryServiceConnection()
        {
            ConnectionStringType connType = ConnectionStringType.DiscoveryService;
            var conn = string.IsNullOrEmpty(DiscoveryServiceConnectionString)
                           ? CreateConnectionFromConnectionStringInConfigFile(DiscoveryConnectionStringKey, connType)
                           : CreateConnectionFromConnectionString(DiscoveryServiceConnectionString, connType);

            return conn;
        }

        protected CrmConnection CreateConnectionFromConnectionStringInConfigFile(string key, ConnectionStringType connType)
        {
            var connStringSetting = ConfigurationManager.ConnectionStrings[key];
            if (connStringSetting == null)
            {
                throw new ArgumentException("Connection string for a required Crm service was not found in the connectionStrings section of your config file. The missing connection string name is:" + key);
            }
            return CreateConnectionFromConnectionString(connStringSetting.ConnectionString, connType);
        }

        protected CrmConnection CreateConnectionFromConnectionString(string connectionString, ConnectionStringType connType)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.");
            }

            var conn = CrmConnection.Parse(connectionString);
            switch (connType)
            {
                case ConnectionStringType.OrgService:
                    this.OrganisationServiceConnectionString = connectionString;
                    break;
                case ConnectionStringType.DeploymentService:
                    this.DeploymentServiceConnectionString = connectionString;
                    break;
                case ConnectionStringType.DiscoveryService:
                    this.DiscoveryServiceConnectionString = connectionString;
                    break;
            }

            return conn;
        }

        protected enum ConnectionStringType
        {
            OrgService,
            DeploymentService,
            DiscoveryService
        }
    }
}