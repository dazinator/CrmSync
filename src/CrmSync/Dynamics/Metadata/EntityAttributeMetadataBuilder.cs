using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmSync.Dynamics.Metadata
{
    /// <summary>
    /// Single responsbility: To provide a fluent API for constructing attribute metadata for an entity.
    /// </summary>
    public class EntityAttributeMetadataBuilder
    {
        public EntityMetadataBuilder MetaDataBuilder { get; set; }

        public List<AttributeMetadata> Attributes { get; set; }

        public EntityAttributeMetadataBuilder(EntityMetadataBuilder metadataBuilder)
        {
            MetaDataBuilder = metadataBuilder;
            Attributes = new List<AttributeMetadata>();
        }


        public EntityAttributeMetadataBuilder StringAttribute(string schemaName,  string displayName, string description,
                                                               AttributeRequiredLevel requiredLevel,
                                                               int maxLength, StringFormat format)
        {
            // Define the primary attribute for the entity
            var newAtt = new StringAttributeMetadata
            {
                SchemaName = schemaName,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                MaxLength = maxLength,
                Format = format,
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033)
            };
            this.Attributes.Add(newAtt);
            return this;
        }

        public EntityAttributeMetadataBuilder BooleanAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel, string trueLabel, int trueValue, string falseLabel, int falseValue)
        {
            int languageCode = 1033;
            // Create a boolean attribute
            var boolAttribute = new BooleanAttributeMetadata
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(displayName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                OptionSet = new BooleanOptionSetMetadata(
                    new OptionMetadata(new Label(trueLabel, languageCode), trueValue),
                    new OptionMetadata(new Label(falseLabel, languageCode), falseValue)
                    )
            };
            this.Attributes.Add(boolAttribute);
            return this;
        }

        public EntityAttributeMetadataBuilder DateTimeAttribute(string schemaName, string displayName, string description,
                                                                 AttributeRequiredLevel requiredLevel,
                                                                 DateTimeFormat format, ImeMode imeMode)
        {
            int languageCode = 1033;
            // Create a date time attribute
            var dtAttribute = new DateTimeAttributeMetadata
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(displayName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                Format = format,
                ImeMode = imeMode
            };
            this.Attributes.Add(dtAttribute);
            return this;
        }
        
        public EntityAttributeMetadataBuilder IntAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel, IntegerFormat format, int min, int max)
        {
            // Define the primary attribute for the entity
            // Create a integer attribute	
            int languageCode = 1033;
            var integerAttribute = new IntegerAttributeMetadata
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(schemaName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                Format = format,
                MaxValue = max,
                MinValue = min
            };

            this.Attributes.Add(integerAttribute);
            return this;
        }
        
        public EntityAttributeMetadataBuilder BigIntAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel)
        {
            // Define the primary attribute for the entity
            var newAtt = new BigIntAttributeMetadata()
            { 
                SchemaName = schemaName,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033)
            };
            this.Attributes.Add(newAtt);
            return this;
        }

        public EntityAttributeMetadataBuilder DecimalAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel, decimal? min, decimal? max, int? precision)
        {
            // Define the primary attribute for the entity
            // Create a integer attribute	
            int languageCode = 1033;
            var att = new DecimalAttributeMetadata()
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(schemaName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                Precision = precision,
                MaxValue = max,
                MinValue = min
            };

            this.Attributes.Add(att);
            return this;
        }


    }
}