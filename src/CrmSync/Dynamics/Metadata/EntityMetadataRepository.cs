using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;
using Microsoft.Xrm.Sdk.Query;

namespace CrmSync.Dynamics.Metadata
{


    /// <summary>
    /// A repository, providing access to metadata for Dynamics Crm entities.
    /// </summary>
    public class EntityMetadataRepository : IEntityMetadataRepository
    {
        private ICrmServiceProvider _CrmServiceProvider;

        public EntityMetadataRepository(ICrmServiceProvider crmServiceProvider)
        {
            _CrmServiceProvider = crmServiceProvider;
        }

        /// <summary>
        /// Gets all metadata for the specified entity.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        public EntityMetadata GetEntityMetadata(string entityLogicalName)
        {
            return this.GetEntityMetadata(entityLogicalName, EntityFilters.All);
        }

        /// <summary>
        /// Gets a subset of metadata for the specified entity, as dictated by the filters specified.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        public EntityMetadata GetEntityMetadata(string entityLogicalName, EntityFilters filters)
        {
            var metaRequest = new RetrieveEntityRequest()
                {
                    EntityFilters = EntityFilters.All,
                    LogicalName = entityLogicalName
                };
            try
            {
                IOrganizationService service = _CrmServiceProvider.GetOrganisationService();
                using (service as IDisposable)
                {
                    var metaResponse = (RetrieveEntityResponse)service.Execute(metaRequest);
                    return metaResponse.EntityMetadata;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to obtain CRM metadata for entity: " + entityLogicalName + " as CRM returned a fault. See inner exception for details.", e);
            }
        }

        /// <summary>
        /// Gets details of changes in metadata that have occurred for a particular entity, since the specified time stamp.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="clientTimestamp"></param>
        /// <returns></returns>
        public RetrieveMetadataChangesResponse GetChanges(string entityLogicalName, string clientTimestamp)
        {
            //if (clientTimestamp == null)
            //{
                
            //}
            return this.GetChanges(entityLogicalName, clientTimestamp, null, DeletedMetadataFilters.All);
        }
        
        /// <summary>
        ///  Gets details of changes in metadata that have occurred for a particular entity, since the specified time stamp. Allows a subset of properties to be specified, and filters for information regarding deleted metadata that will be returned.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="clientTimestamp"></param>
        /// <param name="properties"></param>
        /// <param name="deletedFilters"></param>
        /// <returns></returns>
        public RetrieveMetadataChangesResponse GetChanges(string entityLogicalName, string clientTimestamp, IEnumerable<string> properties, DeletedMetadataFilters deletedFilters)
        {
            // retrieve the current timestamp value;
            var entityFilter = new MetadataFilterExpression(LogicalOperator.And);
            entityFilter.Conditions.Add(new MetadataConditionExpression("LogicalName", MetadataConditionOperator.Equals, entityLogicalName));
            var props = new MetadataPropertiesExpression();
            if (properties == null)
            {
                props.AllProperties = true;
            }
            else
            {
                props.PropertyNames.AddRange(properties);
            }

            //LabelQueryExpression labels = new LabelQueryExpression();
            var entityQueryExpression = new EntityQueryExpression()
                {
                    Criteria = entityFilter,
                    Properties = props,
                    AttributeQuery = new AttributeQueryExpression()
                        {
                            Properties = props
                        }
                };

            var response = GetMetadataChanges(entityQueryExpression, clientTimestamp, deletedFilters);
            var timeStamp = response.ServerVersionStamp;
            Debug.WriteLine("Next Timestamp: " + timeStamp);
            return response;
          
        }

        /// <summary>
        /// Executes the query and returns the results.
        /// </summary>
        /// <param name="entityQueryExpression"></param>
        /// <param name="clientVersionStamp"></param>
        /// <param name="deletedMetadataFilter"></param>
        /// <returns></returns>
        private RetrieveMetadataChangesResponse GetMetadataChanges(EntityQueryExpression entityQueryExpression, String clientVersionStamp, DeletedMetadataFilters deletedMetadataFilter)
        {
            var retrieveMetadataChangesRequest = new RetrieveMetadataChangesRequest()
                {
                    Query = entityQueryExpression,
                    ClientVersionStamp = clientVersionStamp,
                    DeletedMetadataFilters = deletedMetadataFilter
                };
            try
            {
                IOrganizationService service = _CrmServiceProvider.GetOrganisationService();
                using (service as IDisposable)
                {
                    return (RetrieveMetadataChangesResponse)service.Execute(retrieveMetadataChangesRequest);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Unable to obtain changes in CRM metadata using client timestamp: " + clientVersionStamp + " as CRM returned a fault. See inner exception for details.", e);
            }
           
        }



       
           
    
    }
}