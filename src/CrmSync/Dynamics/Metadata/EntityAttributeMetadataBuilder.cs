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

        public StringAttributeMetadata PrimaryNameAttribute { get; set; }

        public EntityAttributeMetadataBuilder(EntityMetadataBuilder metadataBuilder)
        {
            MetaDataBuilder = metadataBuilder;
            Attributes = new List<AttributeMetadata>();
        }

        public EntityAttributeMetadataBuilder NameAttribute(string schemaName, string logicalName, string displayName, string description,
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
                Description = new Label(description, 1033),
                LogicalName = logicalName
            };
            PrimaryNameAttribute = newAtt;
            return this;
        }

        public EntityAttributeMetadataBuilder StringAttribute(string schemaName, string displayName, string description,
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

        public EntityAttributeMetadataBuilder MemoAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel, int maxLength, StringFormat format, ImeMode imeMode = ImeMode.Disabled)
        {
            // Define the primary attribute for the entity
            var newAtt = new MemoAttributeMetadata
            {
                SchemaName = schemaName,
                DisplayName = new Label(displayName, 1033),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, 1033),
                MaxLength = maxLength,
                Format = format,
                ImeMode = imeMode

            };
            this.Attributes.Add(newAtt);
            return this;
        }

        public EntityAttributeMetadataBuilder MoneyAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel, double? min, double? max, int? precision, int? precisionSource, ImeMode imeMode = ImeMode.Disabled)
        {
            // Define the primary attribute for the entity
            // Create a integer attribute	
            int languageCode = 1033;
            var att = new MoneyAttributeMetadata()
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(schemaName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                Precision = precision,
                PrecisionSource = precisionSource,
                MaxValue = max,
                MinValue = min,
                ImeMode = imeMode
            };

            this.Attributes.Add(att);
            return this;
        }

        public EntityAttributeMetadataBuilder PicklistAttribute(string schemaName, string displayName, string description, AttributeRequiredLevel requiredLevel, bool isGlobal, OptionSetType optionSetType, Dictionary<string, int> optionValues)
        {
            // Define the primary attribute for the entity
            // Create a integer attribute	
            int languageCode = 1033;
            var att = new PicklistAttributeMetadata()
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(schemaName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                OptionSet = new OptionSetMetadata
                {
                    IsGlobal = isGlobal,
                    OptionSetType = optionSetType
                }

            };

            foreach (var optionValue in optionValues)
            {
                att.OptionSet.Options.Add(new OptionMetadata(new Label(optionValue.Key, languageCode), optionValue.Value));
            }

            this.Attributes.Add(att);
            return this;
        }

    }
}