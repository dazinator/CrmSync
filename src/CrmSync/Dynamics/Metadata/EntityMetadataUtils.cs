using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmSync.Dynamics.Metadata
{
    public static class EntityMetadataUtils
    {
        /// <summary>Serialize metadata</summary>
        /// <param name="metaData">Metadata to serialize</param>
        /// <param name="formatting">Formatting, determines if indentation and line feeds are used in the file</param>
        public static string SerializeMetaData(this EntityMetadata metaData, Formatting formatting)
        {

            using (var stringWriter = new StringWriter())
            {

                var serializer = new DataContractSerializer(typeof(EntityMetadata), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                var writer = new XmlTextWriter(stringWriter)
                    {
                        Formatting = formatting
                    };
                serializer.WriteObject(writer, metaData);

                writer.Close();

                return stringWriter.ToString();
            }

        }

        /// <summary>
        /// Deserialises the xml into the EntityMetadata object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static EntityMetadata DeserializeMetaData(XmlReader reader)
        {
            var serializer = new DataContractSerializer(typeof(EntityMetadata), null, int.MaxValue, false, false, null, new KnownTypesResolver());
            var entity = (EntityMetadata)serializer.ReadObject(reader);
            return entity;
        }

        /// <summary>
        /// Get's the non-sdk type, in other words, get's the non dyanmics sdk specific type used to represent this attribute type
        /// in a crm agnostic way.
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static Type GetCrmAgnosticType(this AttributeMetadata metadata)
        {
            switch (metadata.AttributeType.GetValueOrDefault())
            {
                case AttributeTypeCode.BigInt:
                    return typeof(long);
                case AttributeTypeCode.Boolean:
                    return typeof(bool);
                case AttributeTypeCode.CalendarRules:
                    return typeof(string);
                case AttributeTypeCode.Customer:
                    return typeof(Guid);
                case AttributeTypeCode.DateTime:
                    return typeof(DateTime);
                case AttributeTypeCode.Decimal:
                    return typeof(decimal);
                case AttributeTypeCode.Double:
                    return typeof(double);
                case AttributeTypeCode.EntityName:
                    return typeof(string);
                case AttributeTypeCode.Integer:
                    return typeof(int);
                case AttributeTypeCode.Lookup:
                    return typeof(Guid);
                case AttributeTypeCode.ManagedProperty:
                    return typeof(bool);
                case AttributeTypeCode.Memo:
                    return typeof(string);
                case AttributeTypeCode.Money:
                    return typeof(decimal);
                case AttributeTypeCode.Owner:
                    return typeof(Guid);
                case AttributeTypeCode.PartyList:
                    return typeof(string);
                case AttributeTypeCode.Picklist:
                    return typeof(int);
                case AttributeTypeCode.State:
                    return typeof(int);
                case AttributeTypeCode.Status:
                    return typeof(int);
                case AttributeTypeCode.String:
                    return typeof(string);
                case AttributeTypeCode.Uniqueidentifier:
                    return typeof(Guid);
                case AttributeTypeCode.Virtual:
                    return typeof(string);
                default:
                    throw new NotSupportedException();
            }
        }


    }
}