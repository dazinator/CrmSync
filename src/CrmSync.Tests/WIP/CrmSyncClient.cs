using System;
using System.Linq;
using CrmSync.Dynamics;
using CrmSync.Dynamics.Metadata;
using CrmSync.Plugin;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace CrmSync.Tests.SystemTests
{
    [Obsolete("Work in progress")]
    public class CrmSyncClient
    {

        public const string EntityName = "crmsync_client";
        public const string NameAttributeName = "crmsync_clientname";
        public const string ClientIdentitiferAttributeName = "crmsync_clientidentifier";
        public const string AnchorRequestedOnAttributeName = "crmsync_anchorrequestedon";

        private IOrganizationService _OrganizationService;

        public CrmSyncClient(IOrganizationService orgService, Guid clientIdentifier, string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName))
            {
                throw new ArgumentNullException("friendlyName");
            }
            if (Guid.Empty == clientIdentifier)
            {
                throw new ArgumentOutOfRangeException("clientIdentifier", "client identifier cannot be an empty guid.");
            }
            _OrganizationService = orgService;
            FriendlyName = friendlyName;
            ClientIdentifier = clientIdentifier;
        }

        /// <summary>
        /// Ensures test entity is created in CRM.
        /// </summary>
        protected void EnsureCrmMetadataProvisioned()
        {

            // Check for entity - if it doesn't exist then create it.
            var request = new RetrieveEntityRequest();
            request.RetrieveAsIfPublished = true;
            var entityName = EntityName;
            var nameattributename = NameAttributeName;

            request.LogicalName = entityName;
            RetrieveEntityResponse response = null;
            try
            {
                response = (RetrieveEntityResponse)_OrganizationService.Execute(request);
            }
            catch (Exception e)
            {
                if (e.Message.ToLower().StartsWith("could not find"))
                {
                    response = null;
                }
                else
                {
                    throw;
                }
            }

            if (response == null || response.EntityMetadata == null)
            {
                var createRequest = new CreateEntityRequest();

                var entityBuilder = EntityConstruction.ConstructEntity(entityName);
                createRequest.Entity = entityBuilder
                    .Description("CrmSync Client")
                    .DisplayCollectionName("CrmSync Clients")
                    .DisplayName("CrmSync Client")
                    .WithAttributes()
                    .StringAttribute(nameattributename, "Client Name", "CrmSync client name", AttributeRequiredLevel.ApplicationRequired, 255, StringFormat.Text)
                    .StringAttribute(ClientIdentitiferAttributeName, "Client Identifer", "CrmSync client identifier", AttributeRequiredLevel.ApplicationRequired, 60, StringFormat.Text)
                    .DateTimeAttribute(AnchorRequestedOnAttributeName, "Anchor Requested On", "Anchor Requested On", AttributeRequiredLevel.ApplicationRequired, DateTimeFormat.DateAndTime, ImeMode.Auto)
                    .MetaDataBuilder.Build();

                createRequest.PrimaryAttribute = (StringAttributeMetadata)entityBuilder.AttributeBuilder.Attributes[0];
                //  createRequest.SolutionUniqueName =
                //try
                //{
                var createResponse = (CreateEntityResponse)_OrganizationService.Execute(createRequest);
                foreach (var att in entityBuilder.AttributeBuilder.Attributes.Where(a => a.SchemaName != nameattributename))
                {
                    var createAttributeRequest = new CreateAttributeRequest
                        {
                            EntityName = entityBuilder.Entity.LogicalName,
                            Attribute = att
                        };
                    var createAttResponse = (CreateAttributeResponse)_OrganizationService.Execute(createAttributeRequest);
                }
                //}
                //catch (Exception e)
                //{
                //    throw;
                //}

            }
        }

        public Guid ClientIdentifier { get; set; }

        public string FriendlyName { get; set; }

        protected Guid? Id { get; set; }

        public void EnsureRegisteredWithServer()
        {
            var existsCheck = IsClientRegistered();
            if (existsCheck.Exists)
            {
                // this client is allready registered with the server.
                Id = existsCheck.EntityReference.Id;
            }
            else
            {
                // register.
                var ent = this.ToEntity();
                var newId = _OrganizationService.Create(ent);
                Id = newId;
            }
        }

        private Entity ToEntity()
        {
            var ent = new Entity(EntityName);
            if (Id != null)
            {
                ent.Id = Id.Value;
            }
            ent[NameAttributeName] = FriendlyName;
            ent[ClientIdentitiferAttributeName] = ClientIdentifier;
            return ent;
        }

        private EntityExists IsClientRegistered()
        {
            var crit = new FilterExpression(LogicalOperator.And);
            crit.AddCondition(ClientIdentitiferAttributeName, ConditionOperator.Equal, ClientIdentifier);

            var existingClient = _OrganizationService.RetrieveMultiple(new QueryExpression(EntityName)
                {
                    ColumnSet = new ColumnSet(NameAttributeName),
                    Criteria = crit
                });

            if (existingClient != null && existingClient.Entities != null && existingClient.Entities.Any())
            {
                if (existingClient.Entities.Count > 1)
                {
                    throw new Exception("There is more than 1 CrmSync client registered with the same server using the exact same client id. Each client should be registered using its own id.");
                }
                var client = existingClient.Entities[0];
                return EntityExists.Yes(new EntityReference(EntityName, client.Id));
            }
            else
            {
                return EntityExists.No();
            }

        }

        public long GetSystemAnchor()
        {
            var entity = this.ToEntity();
            entity[AnchorRequestedOnAttributeName] = DateTime.UtcNow;

            _OrganizationService.Update(entity);
            var newAnchor = _OrganizationService.Retrieve(EntityName, entity.Id,
                                                          new ColumnSet(
                                                              SyncColumnInfo.RowVersionAttributeName));
            if (newAnchor == null)
            {
                throw new InvalidOperationException(string.Format("CrmSync client record with id {0} not found.", entity.Id.ToString()));
            }
            var rowVersion = (long)newAnchor[SyncColumnInfo.RowVersionAttributeName];
            return rowVersion;
        }

    }
}