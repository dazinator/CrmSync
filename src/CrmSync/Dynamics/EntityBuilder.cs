using System;
using System.Collections.Generic;
using System.Linq;
using CrmSync.Dynamics.Metadata;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmSync.Dynamics
{
    /// <summary>
    /// Responsibility: To provide a fluent API for building sdk entities in terms of updating and setting attributes and other values intelligently.
    /// </summary>
    public class EntityBuilder
    {

        private ICrmMetaDataProvider _MetadataProvider;
        private Dictionary<string, EntityAttributeBuilder> _AttributeBuilders;

        protected EntityBuilder(ICrmMetaDataProvider metadataProvider, string entityName)
            : this(metadataProvider, new Entity(entityName))
        {
        }
        protected EntityBuilder(ICrmMetaDataProvider metadataProvider, Entity entity)
        {
            _MetadataProvider = metadataProvider;
            Entity = entity;
            EntityMetadata = _MetadataProvider.GetEntityMetadata(Entity.LogicalName);
            _AttributeBuilders = new Dictionary<string, EntityAttributeBuilder>();
        }

        #region Factory Methods

        public static EntityBuilder WithNewEntity(ICrmMetaDataProvider metadataProvider, string entityName)
        {
            var builder = new EntityBuilder(metadataProvider, entityName);
            return builder;
        }

        public static EntityBuilder WithExistingEntity(ICrmMetaDataProvider metadataProvider, Entity entity)
        {
            var builder = new EntityBuilder(metadataProvider, entity);
            return builder;
        }

        #endregion

        protected Entity Entity { get; set; }

        public CrmEntityMetadata EntityMetadata { get; protected set; }

        public Entity Build()
        {
            return Entity;
        }

        public EntityAttributeBuilder WithAttribute(string logicalName)
        {
            if (!_AttributeBuilders.ContainsKey(logicalName))
            {
                var attMeta = this.EntityMetadata.Attributes.FirstOrDefault(a => a.LogicalName == logicalName);
                if (attMeta == null)
                {
                    // No such column 
                    throw new ArgumentException("Entity: " + Entity.LogicalName + " has no such metadata for an attribute named: " + logicalName);
                }
                var attBuilder = new EntityAttributeBuilder(this, attMeta);
                _AttributeBuilders.Add(logicalName, attBuilder);
                return attBuilder;
            }
            return _AttributeBuilders[logicalName];
        }

        #region nested class


        /// <summary>
        /// Single responsbility: To provide a fluent API for setting / updating an attribute of an entity.
        /// </summary>
        public class EntityAttributeBuilder
        {
            protected DynamicsAttributeTypeProvider AttributeTypeConverter { get; set; }

            public EntityBuilder EntityBuilder { get; set; }

            public AttributeMetadata AttributeMetadata { get; set; }

            public EntityAttributeBuilder(EntityBuilder entityBuilder, AttributeMetadata attributeMetadata)
            {
                EntityBuilder = entityBuilder;
                AttributeMetadata = attributeMetadata;
                AttributeTypeConverter = new DynamicsAttributeTypeProvider();
            }

            public EntityBuilder SetNull()
            {
                this.EntityBuilder.Entity[AttributeMetadata.LogicalName] = null;
                return this.EntityBuilder;
            }

            /// <summary>
            /// Excludes the attribute from modifications by removing it from the entity.
            /// </summary>
            /// <returns></returns>
            public EntityBuilder Exclude()
            {
                EntityBuilder.Entity.Attributes.Remove(AttributeMetadata.LogicalName);
                return EntityBuilder;
            }

            public EntityBuilder SetValue<T>(T val)
            {
                this.EntityBuilder.Entity[AttributeMetadata.LogicalName] = val;
                if (AttributeMetadata.IsPrimaryId.GetValueOrDefault())
                {
                    this.EntityBuilder.Entity.Id = AttributeTypeConverter.GetUniqueIdentifier(val);
                }
                return this.EntityBuilder;
            }

            /// <summary>
            /// Set's the appropriate SDK value of the attribute
            /// </summary>
            /// <param name="value">Will be coerced to the correct sdk type if the type is not appropriate for the attributes underlying metadata.</param>
            /// <returns></returns>
            public EntityBuilder SetValueWithTypeCoersion(object value)
            {

                var meta = this.AttributeMetadata;
                if (value == null || value is DBNull)
                {
                    return this.SetNull();
                }

                if (meta.AttributeType != null)
                {
                    switch (meta.AttributeType.Value)
                    {
                        case AttributeTypeCode.BigInt:
                            SetValue(AttributeTypeConverter.GetBigInt(value));
                            break;
                        case AttributeTypeCode.Boolean:
                            SetValue(AttributeTypeConverter.GetBoolean(value));
                            break;
                        case AttributeTypeCode.CalendarRules:
                            SetValue(AttributeTypeConverter.GetCalendarRules(value));
                            break;
                        case AttributeTypeCode.Customer:
                            SetValue(AttributeTypeConverter.GetCustomer(value));
                            break;
                        case AttributeTypeCode.DateTime:
                            SetValue(AttributeTypeConverter.GetDateTime(value));
                            break;
                        case AttributeTypeCode.Decimal:
                            SetValue(AttributeTypeConverter.GetDecimal(value));
                            break;
                        case AttributeTypeCode.Double:
                            SetValue(AttributeTypeConverter.GetDouble(value));
                            break;
                        case AttributeTypeCode.EntityName:
                            SetValue(AttributeTypeConverter.GetEntityName(value));
                            break;
                        case AttributeTypeCode.Integer:
                            SetValue(AttributeTypeConverter.GetInteger(value));
                            break;
                        case AttributeTypeCode.Lookup:
                            SetValue(AttributeTypeConverter.GetLookup(value));
                            break;
                        case AttributeTypeCode.ManagedProperty:
                            SetValue(AttributeTypeConverter.GetManagedProperty(value));
                            break;
                        case AttributeTypeCode.Memo:
                            SetValue(AttributeTypeConverter.GetMemo(value));
                            break;
                        case AttributeTypeCode.Money:
                            SetValue(AttributeTypeConverter.GetMoney(value));
                            break;
                        case AttributeTypeCode.Owner:
                            SetValue(AttributeTypeConverter.GetOwner(value));
                            break;
                        case AttributeTypeCode.PartyList:
                            SetValue(AttributeTypeConverter.GetPartyList(value));
                            break;
                        case AttributeTypeCode.Picklist:
                            SetValue(AttributeTypeConverter.GetPicklist(value));
                            break;
                        case AttributeTypeCode.State:
                            SetValue(AttributeTypeConverter.GetState(value));
                            break;
                        case AttributeTypeCode.Status:
                            SetValue(AttributeTypeConverter.GetStatus(value));
                            break;
                        case AttributeTypeCode.String:
                            SetValue(AttributeTypeConverter.GetString(value));
                            break;
                        case AttributeTypeCode.Uniqueidentifier:
                            SetValue(AttributeTypeConverter.GetUniqueIdentifier(value));
                            break;
                        case AttributeTypeCode.Virtual:
                            SetValue(AttributeTypeConverter.GetVirtual(value));
                            break;
                        default:
                            throw new NotSupportedException("attribute type: " + meta.AttributeType.ToString() + " is not supoorted.");
                    }
                }

                return EntityBuilder;
            }
        }

        #endregion

    }
}