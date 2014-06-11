namespace CrmSync.Dynamics.Metadata
{
    public interface ICrmMetaDataProvider
    {
        /// <summary>
        /// Returns the metadata for an entity.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        CrmEntityMetadata GetEntityMetadata(string entityName);

        /// <summary>
        /// Ensures the metadata is refreshed and uptodate and returns it.
        /// </summary>
        /// <param name="entityName"></param>
        /// <returns></returns>
        CrmEntityMetadata RefreshEntityMetadata(string entityName);
    }
}