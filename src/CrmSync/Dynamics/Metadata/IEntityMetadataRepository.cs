using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Metadata.Query;

namespace CrmSync.Dynamics.Metadata
{
    public interface IEntityMetadataRepository
    {
        /// <summary>
        /// Returns metadata for the specified entity.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <returns></returns>
        EntityMetadata GetEntityMetadata(string entityLogicalName);

        /// <summary>
        /// Returns filtered metadata for the specified entity.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="filters"></param>
        /// <returns></returns>
        EntityMetadata GetEntityMetadata(string entityLogicalName, EntityFilters filters);

        /// <summary>
        /// Returns only changes in the metadata for a particular entity since the last timestamp.
        /// </summary>
        /// <param name="entityLogicalName"></param>
        /// <param name="clientTimestamp"></param>
        /// <returns></returns>
        RetrieveMetadataChangesResponse GetChanges(string entityLogicalName, string clientTimestamp);

        RetrieveMetadataChangesResponse GetChanges(string entityLogicalName, string clientTimestamp,
                                                   IEnumerable<string> properties, DeletedMetadataFilters deletedFilters);

    }
}