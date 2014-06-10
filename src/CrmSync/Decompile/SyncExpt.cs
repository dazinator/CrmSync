using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;

namespace CrmSync
{
    internal class SyncExpt
    {
        internal static ArgumentException Argument(string error)
        {
            ArgumentException argumentException = new ArgumentException(error);
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentException
                });
            return argumentException;
        }

        internal static ArgumentException Argument(string error, string parameter)
        {
            ArgumentException argumentException = new ArgumentException(error, parameter);
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentException
                });
            return argumentException;
        }

        internal static ArgumentNullException ArgumentNull(string parameter)
        {
            ArgumentNullException argumentNullException = new ArgumentNullException(parameter);
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentNullException
                });
            return argumentNullException;
        }

        internal static ArgumentOutOfRangeException ArgumentOutOfRange(string parameter)
        {
            ArgumentOutOfRangeException ofRangeException = new ArgumentOutOfRangeException(parameter);
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) ofRangeException
                });
            return ofRangeException;
        }

        internal static InvalidOperationException InvalidOperation(string error)
        {
            InvalidOperationException operationException = new InvalidOperationException(error);
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static ArgumentOutOfRangeException InvalidEnumerationValue(Type type, int value)
        {
            return SyncExpt.ArgumentOutOfRange(SyncResource.FormatString("InvalidEnumValue", (object)type.ToString(), (object)value.ToString((IFormatProvider)CultureInfo.InvariantCulture)));
        }

        internal static void CheckArgumentNull(object value, string parameterName)
        {
            if (value == null)
                throw SyncExpt.ArgumentNull(parameterName);
        }

        internal static Exception InvalidSyncTableName(string tableName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("InvalidTableName", new object[1]
                {
                    (object) tableName
                }));
        }

        internal static Exception DuplicateSyncTable(string tableName, string syncGroupName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("SyncTableAlreadyExists", (object)tableName, (object)syncGroupName));
        }

        internal static Exception SyncTableBelongsToDifferentCollection(string tableName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("SyncTableParentExists", new object[1]
                {
                    (object) tableName
                }));
        }

        internal static Exception InvalidSyncGroupName()
        {
            return (Exception)SyncExpt.Argument(SyncResource.GetString("InvalidGroupName"), "GroupName");
        }

        internal static Exception DuplicateSyncAdapter(string tableName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("SyncAdapterAlreadyExists", new object[1]
                {
                    (object) tableName
                }));
        }

        internal static Exception SyncAdapterBelongsToDifferentCollection(string tableName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("SyncAdapterParentExists", new object[1]
                {
                    (object) tableName
                }));
        }

        internal static Exception InvalidSyncAdapterObject()
        {
            return (Exception)SyncExpt.Argument(SyncResource.GetString("SyncAdapterCollection_Add_InvalidType"));
        }

        internal static Exception InvalidSyncParameterName(string parameterName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("InvalidParamName", new object[1]
                {
                    (object) parameterName
                }));
        }

        internal static Exception InvalidScopeIdType(string command, string parameterName, string parameterType)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("InvalidScopeIdType", (object)command, (object)parameterName, (object)parameterType));
        }

        internal static Exception DuplicateSyncParameterName(string parameterName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("DuplicateParamName", new object[1]
                {
                    (object) parameterName
                }));
        }

        internal static Exception InvalidSyncParameterObject()
        {
            return (Exception)SyncExpt.Argument(SyncResource.GetString("InvalidParamObject"));
        }

        internal static Exception SqlChangeTrackingNotEnabled(string tableName)
        {
            return (Exception)SyncExpt.Argument(SyncResource.FormatString("SqlChangeTrackingNotEnabled", new object[1]
                {
                    (object) tableName
                }));
        }

        internal static DataSyncException EnumeratingChangesError(string source, string helpLink, Exception inner, SyncStage stage)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.GetString("ClientSyncProvider_GetChanges_Failed"), inner);
            dataSyncException.SyncSource = source;
            dataSyncException.SyncStage = stage;
            dataSyncException.ErrorNumber = SyncErrorNumber.StoreException;
            dataSyncException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static DataSyncException EnumeratingInsertsError(string source, string helpLink, Exception inner, string tableName, string groupName)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.FormatString("ServerSyncProvider_GetChanges_Failed", (object)tableName, (object)groupName), inner);
            dataSyncException.SyncSource = source;
            dataSyncException.SyncStage = SyncStage.GettingInserts;
            dataSyncException.ErrorNumber = SyncErrorNumber.StoreException;
            dataSyncException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static DataSyncException EnumeratingUpdatesError(string source, string helpLink, Exception inner, string tableName, string groupName)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.FormatString("ServerSyncProvider_GetChanges_Failed", (object)tableName, (object)groupName), inner);
            dataSyncException.SyncSource = source;
            dataSyncException.SyncStage = SyncStage.GettingUpdates;
            dataSyncException.ErrorNumber = SyncErrorNumber.StoreException;
            dataSyncException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static DataSyncException EnumeratingDeletesError(string source, string helpLink, Exception inner, string tableName, string groupName)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.FormatString("ServerSyncProvider_GetChanges_Failed", (object)tableName, (object)groupName), inner);
            dataSyncException.SyncSource = source;
            dataSyncException.SyncStage = SyncStage.GettingDeletes;
            dataSyncException.ErrorNumber = SyncErrorNumber.StoreException;
            dataSyncException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static DataSyncException MissingPrimaryKeyValue(string tableName, string primaryKeyName, SyncStage stage, string source, string helpLink)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.FormatString("MissingPrimaryKeyValue", (object)primaryKeyName, (object)tableName));
            dataSyncException.SyncSource = source;
            dataSyncException.SyncStage = stage;
            dataSyncException.ErrorNumber = SyncErrorNumber.PrimaryKeyNotFound;
            dataSyncException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static InvalidOperationException InvalidSyncMetadataError(string obj1, string obj2, string tableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("InvalidSyncMetadata", (object)obj1, (object)obj2, (object)tableName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static InvalidOperationException NoTableNameError()
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.GetString("NoTableName"));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static InvalidOperationException InvalidCreationOptionDirectionError(string tableName, TableCreationOption creationOption, SyncDirection direction)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("InvalidCreationOptionDirection", (object)tableName, (object)((object)creationOption).ToString(), (object)((object)direction).ToString()));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static ArgumentException ColumnNotInTable(string table, string column)
        {
            return SyncExpt.Argument(SyncResource.FormatString("SyncSchema_ColumnNotInTable", (object)column, (object)table));
        }

        internal static InvalidOperationException CannotRemoveTable(string tableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("SyncSchema_CannotRemoveTable", new object[1]
                {
                    (object) tableName
                }));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static InvalidOperationException CannotRemoveColumn(string columnName, string tableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("SyncSchema_CannotRemoveColumn", (object)columnName, (object)tableName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static SchemaException InvalidSchemaDataSetError(string tableName, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.GetString("ServerSyncProvider_Invalid_Schema_dataset"));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.ReadingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.InvalidSchemaDataSet;
            schemaException.TableName = tableName;
            schemaException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException ReadingSchemaFromDataSetError(Collection<string> tableNames, string source, string helpLink, SyncErrorNumber code, Exception inner)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (string str in tableNames)
                stringBuilder.Append(str).Append(", ");
            string str1 = "";
            if (stringBuilder.Length > 2 && ((object)stringBuilder).ToString().EndsWith(", ", StringComparison.Ordinal))
                str1 = ((object)stringBuilder).ToString().Substring(0, stringBuilder.Length - 2);
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ServerSyncProvider_GetSchemaFromDatabase_Failed", new object[1]
                {
                    (object) str1
                }), inner);
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.ReadingSchema;
            schemaException.ErrorNumber = code;
            schemaException.TableName = str1;
            schemaException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException MissingSelectStatementError(string tableName, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.GetString("SyncAdapter_FillSchema_SelectCommandMissing"));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.ReadingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.MissingSelectCommand;
            schemaException.TableName = tableName;
            schemaException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException FillSchemaError(string tableName, string source, string helpLink, Exception inner)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ServerSyncProvider_GetSchemaFromDatabase_Failed", new object[1]
                {
                    (object) tableName
                }), inner);
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.ReadingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.StoreException;
            schemaException.TableName = tableName;
            schemaException.HelpLink = helpLink;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException PrimaryKeyNotFoundError(string tableName, SyncStage stage, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("PrimaryKeyNotFound", new object[1]
                {
                    (object) tableName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = stage;
            schemaException.ErrorNumber = SyncErrorNumber.PrimaryKeyNotFound;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException MissingColumnDefinition(string tableName, string columnName, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("MissingColumnDefinition", (object)columnName, (object)tableName));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.InvalidValue;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException InsufficientColumnInformationError(string tableName, string columnName, string property, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("InsufficientColumnInformation", (object)columnName, (object)tableName, (object)property));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.InvalidValue;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException InvalidTrackingColumnType(string tableName, string columnName, string source)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("InvalidTrackingColumn", new object[1]
                {
                    (object) columnName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.ReadingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.InvalidValue;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException InvalidOriginatorIdColumnType(string tableName, string columnName, string source)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("InvalidOriginatorIdColumn", new object[1]
                {
                    (object) columnName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.ReadingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.InvalidValue;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException TableAlreadyExistsError(string tableName, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("TableAlreadyExists", new object[1]
                {
                    (object) tableName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.TableAlreadyExists;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException TableDoesNotExistError(string tableName, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("TableDoesNotExist", new object[1]
                {
                    (object) tableName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.TableDoesNotExist;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException CreatingTableError(string tableName, string createScript, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ErrorCreatingTable", (object)tableName, (object)createScript));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.ParsingQueryFailed;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException CreatingTableError(string tableName, string createScript, string source, string helpLink, Exception inner)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ErrorCreatingTable", (object)tableName, (object)createScript), inner);
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.StoreException;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException EstablishingRelationError(ForeignKeyConstraint fk, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ErrorEstablishingRelation", new object[1]
                {
                    (object) fk.ConstraintName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.InvalidArguments;
            schemaException.HelpLink = helpLink;
            if (fk.RelatedTable != null)
                schemaException.TableName = fk.RelatedTable.TableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException EstablishingRelationError(ForeignKeyConstraint fk, string commandText, string source, string helpLink, Exception inner)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ErrorEstablishingRelationEx", (object)fk.ConstraintName, (object)commandText), inner);
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.CreatingSchema;
            schemaException.ErrorNumber = SyncErrorNumber.StoreException;
            schemaException.HelpLink = helpLink;
            if (fk.RelatedTable != null)
                schemaException.TableName = fk.RelatedTable.TableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static SchemaException SchemaNotMatchError(string tableName, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ServerSyncProvider_GetChanges_SchemaNotMatch", new object[1]
                {
                    (object) tableName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = SyncStage.DownloadingChanges;
            schemaException.ErrorNumber = SyncErrorNumber.StoreException;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static MetadataException CreateSystemTablesError(string tableName, string source, string helpLink, Exception inner)
        {
            MetadataException metadataException = new MetadataException(SyncResource.FormatString("Internal_ErrorCreatingSystemTables", new object[1]
                {
                    (object) tableName
                }), inner);
            metadataException.SyncSource = source;
            metadataException.SyncStage = SyncStage.CreatingMetadata;
            metadataException.ErrorNumber = SyncErrorNumber.StoreException;
            metadataException.HelpLink = helpLink;
            metadataException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) metadataException
                });
            return metadataException;
        }

        internal static MetadataException SubscribingTableError(string tableName, string source, string helpLink)
        {
            MetadataException metadataException = new MetadataException(SyncResource.FormatString("Internal_ErrorInSubscribingTable", new object[1]
                {
                    (object) tableName
                }));
            metadataException.SyncSource = source;
            metadataException.SyncStage = SyncStage.WritingMetadata;
            metadataException.ErrorNumber = SyncErrorNumber.AddingTableMetadataFailed;
            metadataException.HelpLink = helpLink;
            metadataException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) metadataException
                });
            return metadataException;
        }

        internal static MetadataException ReadingTableAnchorError(string tableName, string anchorType, string source, string helpLink, Exception inner)
        {
            MetadataException metadataException = new MetadataException(SyncResource.FormatString("Internal_GetTableAnchorFailed", (object)anchorType, (object)tableName), inner);
            metadataException.SyncSource = source;
            metadataException.SyncStage = SyncStage.ReadingMetadata;
            metadataException.ErrorNumber = SyncErrorNumber.StoreException;
            metadataException.HelpLink = helpLink;
            metadataException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) metadataException
                });
            return metadataException;
        }

        internal static MetadataException WritingTableAnchorError(string tableName, string anchorType, string source, string helpLink, Exception inner)
        {
            MetadataException metadataException = new MetadataException(SyncResource.FormatString("Internal_SetTableAnchorFailed", (object)anchorType, (object)tableName), inner);
            metadataException.SyncSource = source;
            metadataException.SyncStage = SyncStage.WritingMetadata;
            metadataException.ErrorNumber = SyncErrorNumber.StoreException;
            metadataException.HelpLink = helpLink;
            metadataException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) metadataException
                });
            return metadataException;
        }

        internal static MetadataException WritingTableAnchorError(string tableName, string anchorType, string source, string helpLink, SyncErrorNumber code)
        {
            MetadataException metadataException = new MetadataException(SyncResource.FormatString("Internal_SetTableAnchorFailed", (object)anchorType, (object)tableName));
            metadataException.SyncSource = source;
            metadataException.SyncStage = SyncStage.WritingMetadata;
            metadataException.ErrorNumber = code;
            metadataException.HelpLink = helpLink;
            metadataException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) metadataException
                });
            return metadataException;
        }

        internal static SchemaException ClientSchemaNotMatchError(string tableName, SyncStage stage, string source, string helpLink)
        {
            SchemaException schemaException = new SchemaException(SyncResource.FormatString("ApplyChanges_ClientSchemaNotMatchError", new object[1]
                {
                    (object) tableName
                }));
            schemaException.SyncSource = source;
            schemaException.SyncStage = stage;
            schemaException.ErrorNumber = SyncErrorNumber.StoreException;
            schemaException.HelpLink = helpLink;
            schemaException.TableName = tableName;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) schemaException
                });
            return schemaException;
        }

        internal static DataSyncException AnotherSyncInProgressError(string source, string helpLink)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.GetString("SyncAlreadyStarted"), (Exception)null);
            dataSyncException.SyncSource = source;
            dataSyncException.HelpLink = helpLink;
            dataSyncException.SyncStage = SyncStage.ReadingMetadata;
            dataSyncException.ErrorNumber = SyncErrorNumber.SyncInProgress;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static DataSyncException FailedToExecuteCommand(string command, string table, Exception expt, string source, string helpLink, SyncStage stage)
        {
            DataSyncException dataSyncException = new DataSyncException(SyncResource.FormatString("FailedToExecuteCommand", (object)command, (object)table), expt);
            dataSyncException.SyncSource = source;
            dataSyncException.HelpLink = helpLink;
            dataSyncException.SyncStage = stage;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) dataSyncException
                });
            return dataSyncException;
        }

        internal static AnchorException InvalidAnchorValueError(SyncGroupMetadata metadata, SyncSession session, string source, string helpLink)
        {
            AnchorException anchorException = new AnchorException(SyncResource.GetString("GetNewServerAnchorFailed"));
            anchorException.SyncSource = source;
            anchorException.SyncStage = SyncStage.DownloadingChanges;
            anchorException.ErrorNumber = SyncErrorNumber.InvalidValue;
            anchorException.GroupMetadata = metadata;
            anchorException.HelpLink = helpLink;
            anchorException.Session = session;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) anchorException
                });
            return anchorException;
        }

        internal static AnchorException ReadingAnchorError(SyncGroupMetadata metadata, SyncSession session, string source, string helpLink, Exception inner)
        {
            AnchorException anchorException = new AnchorException(SyncResource.GetString("GetNewServerAnchorFailed"), inner);
            anchorException.SyncSource = source;
            anchorException.SyncStage = SyncStage.DownloadingChanges;
            anchorException.ErrorNumber = SyncErrorNumber.StoreException;
            anchorException.GroupMetadata = metadata;
            anchorException.HelpLink = helpLink;
            anchorException.Session = session;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) anchorException
                });
            return anchorException;
        }

        internal static ArgumentException ParameterNotSet(string parameter)
        {
            return SyncExpt.Argument(SyncResource.FormatString("ParameterNotSet", new object[1]
                {
                    (object) parameter
                }));
        }

        internal static SessionVariableException MissingSessionVariableError(DbParameter parameter, SyncStage stage, string source, string commandName, string helpLink)
        {
            SessionVariableException variableException = new SessionVariableException(SyncResource.FormatString("SessionVariableMissing", (object)parameter.ParameterName, (object)commandName));
            variableException.SyncSource = source;
            variableException.HelpLink = helpLink;
            variableException.SyncStage = stage;
            variableException.ErrorNumber = SyncErrorNumber.MissingSessionVariable;
            variableException.Parameter = parameter;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) variableException
                });
            return variableException;
        }

        internal static SessionVariableException FailedToMapOriginatorId(SyncStage stage, string source, string helpLink)
        {
            SessionVariableException variableException = new SessionVariableException(SyncResource.GetString("FailedToMapOriginatorId"));
            variableException.ErrorNumber = SyncErrorNumber.MissingSessionVariable;
            variableException.SyncSource = source;
            variableException.HelpLink = helpLink;
            variableException.SyncStage = stage;
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) variableException
                });
            return variableException;
        }

        internal static ArgumentException ColumnNotExistsInTableError(string tableName, string columnName)
        {
            ArgumentException argumentException = new ArgumentException(SyncResource.FormatString("ColumnNotExistsInTable", (object)tableName, (object)columnName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentException
                });
            return argumentException;
        }

        internal static InvalidOperationException NoQuoteChange(string tableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("NoQuoteChange", new object[1]
                {
                    (object) tableName
                }));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static ArgumentOutOfRangeException InvalidCatalogLocation(CatalogLocation value)
        {
            return SyncExpt.InvalidEnumerationValue(typeof(CatalogLocation), (int)value);
        }

        internal static InvalidOperationException MissingSourceCommandConnection(string tableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("MissingSourceCommandConnection", new object[1]
                {
                    (object) tableName
                }));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static InvalidOperationException DynamicSQLNoTableInfo()
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.GetString("DynamicSQLNoTableInfo"));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static InvalidOperationException DynamicSQLNestedQuote(string name, string quote)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("DynamicSQLNestedQuote", (object)name, (object)quote));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static InvalidOperationException DynamicSQLNoKeyInfo(string tableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("DynamicSQLNoKeyInfo", new object[1]
                {
                    (object) tableName
                }));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static NotSupportedException NotSupported()
        {
            NotSupportedException supportedException = new NotSupportedException();
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) supportedException
                });
            return supportedException;
        }

        internal static NotSupportedException NotSupported(string reason)
        {
            NotSupportedException supportedException = new NotSupportedException(reason);
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) supportedException
                });
            return supportedException;
        }

        internal static ArgumentException SyncMetadataColumnNotExists(string tableName, string columnName, string columnType)
        {
            ArgumentException argumentException = new ArgumentException(SyncResource.FormatString("SyncMetadataColumnNotExists", (object)columnType, (object)columnName, (object)tableName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentException
                });
            return argumentException;
        }

        internal static InvalidOperationException SyncNonMatchingKeys(string baseTableName, string tombstoneTableName)
        {
            InvalidOperationException operationException = new InvalidOperationException(SyncResource.FormatString("SyncNonMatchingKeys", (object)baseTableName, (object)tombstoneTableName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) operationException
                });
            return operationException;
        }

        internal static MissingMemberException MethodNotFoundError(string methodName)
        {
            MissingMemberException missingMemberException = new MissingMemberException(SyncResource.FormatString("MethodNotFound", new object[1]
                {
                    (object) methodName
                }));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) missingMemberException
                });
            return missingMemberException;
        }

        internal static ArgumentException InvalidProviderDataType(string providerType, string columnName, string tableName)
        {
            ArgumentException argumentException = new ArgumentException(SyncResource.FormatString("InvalidProviderDataType", (object)providerType, (object)columnName, (object)tableName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentException
                });
            return argumentException;
        }

        internal static ArgumentException InvalidNumericColumnOperation(string tableName, string columnName)
        {
            ArgumentException argumentException = new ArgumentException(SyncResource.FormatString("SyncSchema_NotNumeric", (object)tableName, (object)columnName));
            SyncTracer.Warning("{0}", new object[1]
                {
                    (object) argumentException
                });
            return argumentException;
        }

        internal static ArgumentException InvalidConnectionArgument()
        {
            return SyncExpt.Argument(SyncResource.GetString("InvalidConnectionArgument"));
        }

        internal static ArgumentException MaxSyncAnchorSize(int maxAnchorSize)
        {
            return SyncExpt.Argument(SyncResource.FormatString("MaxSyncAnchorSize", new object[1]
                {
                    (object) maxAnchorSize.ToString((IFormatProvider) CultureInfo.InvariantCulture)
                }));
        }

        internal static ArgumentException MaxNumberOfCustomParameters(int maxCustomParameters)
        {
            return SyncExpt.Argument(SyncResource.FormatString("MaxNumberOfCustomParameters", new object[1]
                {
                    (object) maxCustomParameters.ToString((IFormatProvider) CultureInfo.InvariantCulture)
                }));
        }

        internal static ArgumentException MaxSizeOfCustomParameter()
        {
            return SyncExpt.Argument(SyncResource.GetString("MaxSizeOfCustomParameter"));
        }

        internal static bool IsFatal(Exception exp)
        {
            if (!(exp is OutOfMemoryException) && !(exp is StackOverflowException))
                return exp is AccessViolationException;
            else
                return true;
        }
    }
}