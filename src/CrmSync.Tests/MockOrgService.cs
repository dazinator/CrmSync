using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmSync.Tests
{
    public class MockOrgService : IOrganizationService
    {
        public MockOrgService()
        {
            CapturedInput = new OrgInput();
            RespondWith = new OrgReponse();
        }

        public OrgInput CapturedInput { get; set; }

        public OrgReponse RespondWith { get; set; }
        
        public virtual Guid Create(Entity entity)
        {
            if (entity.Id == Guid.Empty)
            {
                entity.Id = RespondWith.EntityId;
            }
            CapturedInput.CreateEntity = entity;
            CapturedInput.EntityName = entity.LogicalName;
            return RespondWith.EntityId;
        }

        public virtual Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            CapturedInput.EntityName = entityName;
            CapturedInput.EntityId = id;
            CapturedInput.ColumnSet = columnSet;
            return RespondWith.RetrieveEntity;
        }

        public virtual void Update(Entity entity)
        {
            CapturedInput.EntityId = entity.Id;
            CapturedInput.EntityName = entity.LogicalName;
            CapturedInput.UpdateEntity = entity;
        }

        public virtual void Delete(string entityName, Guid id)
        {
            CapturedInput.EntityName = entityName;
            CapturedInput.EntityId = id;
        }

        public virtual OrganizationResponse Execute(OrganizationRequest request)
        {
            CapturedInput.OrgRequest = request;
            return RespondWith.OrgResponse;
        }

        public virtual void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            CapturedInput.EntityId = entityId;
            CapturedInput.EntityName = entityName;
            CapturedInput.Relationship = relationship;
            CapturedInput.RelatedEntities = relatedEntities;
        }

        public virtual void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            CapturedInput.EntityId = entityId;
            CapturedInput.EntityName = entityName;
            CapturedInput.Relationship = relationship;
            CapturedInput.RelatedEntities = relatedEntities;
        }

        public virtual EntityCollection RetrieveMultiple(QueryBase query)
        {
            CapturedInput.Query = query;
            return RespondWith.EntityCollection;
        }

        public class OrgReponse
        {
            public Guid EntityId { get; set; }

            public Entity RetrieveEntity { get; set; }

            public EntityCollection EntityCollection { get; set; }

            public OrganizationResponse OrgResponse { get; set; }
        }

        public class OrgInput
        {

            public string EntityName { get; set; }

            public Guid EntityId { get; set; }

            public Entity UpdateEntity { get; set; }

            public Entity CreateEntity { get; set; }

            public ColumnSet ColumnSet { get; set; }

            public OrganizationRequest OrgRequest { get; set; }

            public Relationship Relationship { get; set; }

            public EntityReferenceCollection RelatedEntities { get; set; }

            public QueryBase Query { get; set; }

        }
    }
}