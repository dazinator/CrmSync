using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;

namespace CrmSync.Dynamics.Metadata
{
    public class InMemoryCachedCrmMetaDataProvider : ICrmMetaDataProvider
    {
        private static readonly ConcurrentDictionary<string, CrmEntityMetadata> _Metadata = new ConcurrentDictionary<string, CrmEntityMetadata>();

        private IEntityMetadataRepository _repository;

        public InMemoryCachedCrmMetaDataProvider(IEntityMetadataRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Returns the metadata for an entity.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public CrmEntityMetadata GetEntityMetadata(string entityName)
        {
            var changes = _Metadata.GetOrAdd(entityName, p =>
            {
                Debug.WriteLine("Retrieving metadata for entity: " + entityName, "Metadata");
                var metadata = _repository.GetChanges(entityName, null);
                var entMeta = metadata.EntityMetadata[0];
                var result = new CrmEntityMetadata()
                    {
                        Attributes = entMeta.Attributes.ToList(),
                        EntityName = entityName,
                        Timestamp = metadata.ServerVersionStamp
                    };
                return result;
            });

            return changes;
        }
     

        /// <summary>
        /// Ensures the metadata is refreshed and uptodate and returns it.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        public CrmEntityMetadata RefreshEntityMetadata(string entityName)
        {
            bool isPresent = true;
            var result = _Metadata.GetOrAdd(entityName, p =>
                {
                    isPresent = false;
                    var metadata = _repository.GetChanges(entityName, null);
                    var entMeta = metadata.EntityMetadata[0];
                    var crment = new CrmEntityMetadata()
                        {
                            Attributes = entMeta.Attributes.ToList(),
                            EntityName = entityName,
                            Timestamp = metadata.ServerVersionStamp
                        };
                    return crment;
                });


            if (!isPresent)
            {
                // it wasn;t present in the cache, so return the result as its currently the latest.
                return result;
            }
            // refresh the metadata
            // it was present in the cache, so get check for changes and update if required before returning.
            Debug.WriteLine("Refreshing metadata for entity: " + entityName, "Metadata");
            var changes = _repository.GetChanges(entityName, result.Timestamp);
            // update existing metadata..
            var latestEntityMetadata = changes.EntityMetadata[0];

            // Detect new / deleted fields.
            List<AttributeMetadata> modifiedFields = null;
            List<Guid> deletedFields = null;

            if (latestEntityMetadata.HasChanged.GetValueOrDefault())
            {
                modifiedFields = latestEntityMetadata.Attributes.Where(att => att.HasChanged.GetValueOrDefault()).ToList();
                deletedFields = changes.DeletedMetadata != null &&
                                changes.DeletedMetadata.ContainsKey(DeletedMetadataFilters.Attribute)
                                    ? changes.DeletedMetadata[DeletedMetadataFilters.Attribute].ToList()
                                    : null;
                // Work out what was changed..
                // var deletedFields = latestEntityMetadataResponse.DeletedMetadata.Where(att => att.HasChanged.GetValueOrDefault()).ToList();

            }
            
            // Loop through all metadata items, and add missing change units.
            bool hasSchemaChanges = (modifiedFields != null && modifiedFields.Any()) || (deletedFields != null && deletedFields.Any());
            if (hasSchemaChanges)
            {
                Debug.WriteLine("Updating metadata for entity: " + entityName, "Metadata");
                result.Refresh(modifiedFields, deletedFields);
            }

            return result;
        }

      
    }


}
