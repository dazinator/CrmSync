using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Data;

namespace CrmSync
{
    internal static class SyncUtil
    {
        internal const string StrEmpty = "";
        internal const CompareOptions SyncCompareOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth;
        internal const string ProductHelpLink = "http://msdn.microsoft.com/sync";
        internal const uint SupportedKnowledgeVersion = 1U;
        internal const string SchemaMajorVersionColName = "schema_major_version";
        internal const string SchemaMinorVersionColName = "schema_minor_version";
        internal const string SchemaExtendedInfoColName = "schema_extended_info";
        internal const string ScopeLocalIdColName = "scope_local_id";
        internal const string ScopeIdColName = "scope_id";
        internal const string ScopeNameColName = "sync_scope_name";
        internal const string ScopeKnowledgeColName = "scope_sync_knowledge";
        internal const string ScopeForgottenKnowledgeColName = "scope_tombstone_cleanup_knowledge";
        internal const string ScopeTimestampColName = "scope_timestamp";
        internal const string ScopeCleanupTimestampColName = "scope_cleanup_timestamp";
        internal const string ScopeRestoreCountColName = "scope_restore_count";
        internal const string MaxTimestampTableColName = "table_name";
        internal const string MaxTimestampMaxColName = "max_timestamp";
        internal const int DbNullSize = 5;
        internal const int RetryAmount = 5;
        internal const int RetryWaitMilliseconds = 100;

        internal static SyncIdFormatGroup IdFormats()
        {
            return new SyncIdFormatGroup()
                {
                    ChangeUnitIdFormat =
                        {
                            IsVariableLength = false,
                            Length = (ushort)1
                        },
                    ReplicaIdFormat =
                        {
                            IsVariableLength = false,
                            Length = (ushort)16
                        },
                    ItemIdFormat =
                        {
                            IsVariableLength = true,
                            Length = (ushort)10240
                        }
                };
        }

        internal static SqlDbType GetSqlDbTypeFromString(string typeString)
        {
            if (string.Compare(typeString, "sql_variant", StringComparison.OrdinalIgnoreCase) == 0)
                return SqlDbType.Variant;
            if (string.Compare(typeString, "numeric", StringComparison.OrdinalIgnoreCase) == 0)
                return SqlDbType.Decimal;
            else
                return (SqlDbType)Enum.Parse(typeof(SqlDbType), typeString, true);
        }

        internal static void SetLocalTickCountRanges(SyncKnowledge knowledge, ulong newTick)
        {
            SyncKnowledge knowledge1 = knowledge.Clone();
            knowledge.SetLocalTickCount(newTick);
            knowledge.Combine(knowledge1);
        }

        internal static void CheckForAdapterRowIds(DbSyncAdapterCollection adapters)
        {
            List<string> list = new List<string>();
            foreach (DbSyncAdapter dbSyncAdapter in (Collection<DbSyncAdapter>)adapters)
            {
                if (dbSyncAdapter.RowIdColumns.Count == 0)
                    list.Add(dbSyncAdapter.TableName);
            }
            if (list.Count <= 0)
                return;
            StringBuilder stringBuilder = new StringBuilder();
            string str1 = string.Empty;
            foreach (string str2 in list)
            {
                stringBuilder.Append(str1 + str2);
                str1 = ", ";
            }
            SyncTracer.Error(SyncResource.FormatString("MissingAdapterRowIdColumns", new object[0]), new object[1]
                {
                    (object) ((object) stringBuilder).ToString()
                });
            throw new DbSyncException(SyncResource.FormatString("MissingAdapterRowIdColumns", new object[1]
                {
                    (object) ((object) stringBuilder).ToString()
                }));
        }

        internal static bool CheckIfCommandHasOutputParameters(IDbCommand command)
        {
            foreach (IDbDataParameter dbDataParameter in (IEnumerable)command.Parameters)
            {
                if (dbDataParameter.Direction == ParameterDirection.Output || dbDataParameter.Direction == ParameterDirection.InputOutput)
                    return true;
            }
            return false;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        internal static bool OpenConnection(IDbConnection connection)
        {
            SyncExpt.CheckArgumentNull((object)connection, "connection");
            bool flag = false;
            switch (connection.State)
            {
                case ConnectionState.Closed:
                    if (SyncTracer.IsVerboseEnabled())
                    {
                        if (connection is SqlConnection)
                        {
                            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
                            connectionStringBuilder.ConnectionString = connection.ConnectionString;
                            if (!string.IsNullOrEmpty(connectionStringBuilder.Password))
                                connectionStringBuilder.Password = "****";
                            SyncTracer.Verbose("Connecting using string: {0}", new object[1]
                                {
                                    (object) connectionStringBuilder.ConnectionString
                                });
                        }
                        else
                            SyncTracer.Verbose("Connecting to database: {0}", new object[1]
                                {
                                    (object) connection.Database
                                });
                    }
                    if (connection is SqlConnection || connection is IConnectionWrapper)
                        SyncUtil.TryOpenConnection(connection);
                    else
                        connection.Open();
                    flag = true;
                    goto case ConnectionState.Open;
                case ConnectionState.Open:
                    return flag;
                case ConnectionState.Broken:
                    SyncTracer.Verbose("Closing broken connection");
                    connection.Close();
                    goto case ConnectionState.Closed;
                default:
                    throw new DbSyncException(SyncResource.FormatString("UnhandledConnectionState", new object[1]
                        {
                            (object) ((object) connection.State).ToString()
                        }));
            }
        }

        internal static void TryOpenConnection(IDbConnection connection)
        {
            for (int index = 0; index < 6; ++index)
            {
                try
                {
                    if (index > 0)
                        SyncTracer.Info("Retrying opening connection, attempt {0} of {1}.", new object[2]
                            {
                                (object) index,
                                (object) 5
                            });
                    connection.Open();
                    SqlConnection connection1 = connection as SqlConnection;
                    if (connection1 == null)
                        break;
                    using (SqlCommand sqlCommand = new SqlCommand("Select 1", connection1))
                    {
                        sqlCommand.ExecuteScalar();
                        break;
                    }
                }
                catch (SqlException ex)
                {
                    if (index == 5)
                    {
                        SyncTracer.Error("Open connection failed after max retry attempts, due to exception: {0}", new object[1]
                            {
                                (object) ex.Message
                            });
                        throw;
                    }
                    else if (!SyncUtil.RetryLitmus(ex))
                    {
                        SyncTracer.Error("Open connection failed on attempt {0} of {1}, due to unretryable exception: {2}", (object)(index + 1), (object)5, (object)ex.Message);
                        throw;
                    }
                    else
                    {
                        SyncTracer.Warning("Open connection failed on attempt {0} of {1}, due to retryable exception: {2}", (object)(index + 1), (object)5, (object)ex.Message);
                        Thread.Sleep(100 * (int)Math.Pow(2.0, (double)index));
                    }
                }
            }
        }

        internal static bool RetryLitmus(SqlException sqlException)
        {
            switch (sqlException.Number)
            {
                case 40197:
                case 40501:
                case 40613:
                case 64:
                case 10053:
                case 10054:
                case 10060:
                    return true;
                default:
                    return false;
            }
        }

        internal static void ExecuteNonQueryWithNewTransaction(IDbCommand command)
        {
            for (int index = 0; index < 6; ++index)
            {
                try
                {
                    if (index > 0)
                    {
                        SyncTracer.Info("Retrying SyncUtil.ExecuteNonQueryWithNewTransaction, attempt {0} of {1}.", new object[2]
                            {
                                (object) index,
                                (object) 5
                            });
                        SyncUtil.OpenConnection(command.Connection);
                    }
                    using (IDbTransaction dbTransaction = command.Connection.BeginTransaction())
                    {
                        command.Transaction = dbTransaction;
                        command.ExecuteNonQuery();
                        dbTransaction.Commit();
                        break;
                    }
                }
                catch (DbException ex)
                {
                    if (index == 5)
                    {
                        SyncTracer.Error("SyncUtil.ExecuteNonQueryWithNewTransaction failed after max retry attempts, due to exception: {0}", new object[1]
                            {
                                (object) ex.Message
                            });
                        throw;
                    }
                    else
                    {
                        SyncTracer.Warning("SyncUtil.ExecuteNonQueryWithNewTransaction failed on attempt {0} of {1}, due to retryable exception: {2}", (object)index, (object)5, (object)ex.Message);
                        Thread.Sleep(100 * (int)Math.Pow(2.0, (double)index));
                    }
                }
            }
        }

        //internal static IDataReader ExecuteDataReader(IDbCommand command, bool closeConnection, CommandFailureInjector.InjectCommandFailure injectCommandFailure, CommandFailureInjector.InjectCommandFailureRetryResponse failurecommandResponse)
        //{
        //    IDataReader dataReader = (IDataReader)null;
        //    for (int retryAttempts = 0; retryAttempts < 6; ++retryAttempts)
        //    {
        //        try
        //        {
        //            if (injectCommandFailure != null)
        //                injectCommandFailure();
        //            if (retryAttempts > 0)
        //            {
        //                SyncTracer.Info("Retrying SyncUtil.ExecuteDataReader, attempt {0} of {1}.", new object[2]
        //    {
        //      (object) retryAttempts,
        //      (object) 5
        //    });
        //                SyncUtil.OpenConnection(command.Connection);
        //            }
        //            dataReader = !closeConnection ? command.ExecuteReader() : command.ExecuteReader(CommandBehavior.CloseConnection);
        //            if (failurecommandResponse != null)
        //                failurecommandResponse(retryAttempts);
        //            return dataReader;
        //        }
        //        catch (DbException ex)
        //        {
        //            if (retryAttempts == 5)
        //            {
        //                SyncTracer.Error("Retrying SyncUtil.ExecuteDataReader failed after max retry attempts, due to exception: {0}", new object[1]
        //    {
        //      (object) ex.Message
        //    });
        //                throw;
        //            }
        //            else
        //            {
        //                SyncTracer.Warning("SyncUtil.ExecuteDataReader failed on attempt {0} of {1}, due to retryable exception: {2}", (object)retryAttempts, (object)5, (object)ex.Message);
        //                Thread.Sleep(100 * (int)Math.Pow(2.0, (double)retryAttempts));
        //            }
        //        }
        //    }
        //    return dataReader;
        //}

        internal static object GetSyncObjectOutParameter(string parameter, IDbCommand command, out bool found)
        {
            found = true;
            DbParameter parameter1 = SyncUtil.GetParameter(command, parameter);
            if (parameter1 != null)
                return parameter1.Value;
            found = false;
            return (object)null;
        }

        internal static object GetSyncObjectOutParameter(string parameter, IDbCommand command)
        {
            bool found;
            return SyncUtil.GetSyncObjectOutParameter(parameter, command, out found);
        }

        internal static void SetParameterValue(IDbCommand command, string parameterName, object value)
        {
            DbParameter parameter = SyncUtil.GetParameter(command, parameterName);
            if (parameter == null)
                return;
            parameter.Value = value;
        }

        internal static void SetParameterValueAndSize(IDbCommand command, string parameterName, object value, int size)
        {
            DbParameter parameter = SyncUtil.GetParameter(command, parameterName);
            if (parameter == null)
                return;
            parameter.Size = size;
            parameter.Value = value;
        }

        internal static DbParameter GetParameter(IDbCommand command, string parameterName)
        {
            if (command == null)
                return (DbParameter)null;
            if (command.Parameters.Contains("@" + parameterName))
                return (DbParameter)command.Parameters["@" + parameterName];
            if (command.Parameters.Contains(":" + parameterName))
                return (DbParameter)command.Parameters[":" + parameterName];
            if (command.Parameters.Contains(parameterName))
                return (DbParameter)command.Parameters[parameterName];
            else
                return (DbParameter)null;
        }

        internal static string SetIdentityInsertText(bool enable, string quotedTableName)
        {
            return "SET IDENTITY_INSERT " + quotedTableName + (enable ? " ON" : " OFF");
        }

        internal static int GetSyncIntOutParameter(string parameter, IDbCommand command, out bool found)
        {
            found = true;
            DbParameter parameter1 = SyncUtil.GetParameter(command, parameter);
            if (parameter1 != null && parameter1.Value != null && !string.IsNullOrEmpty(parameter1.Value.ToString()))
                return int.Parse(parameter1.Value.ToString(), (IFormatProvider)CultureInfo.InvariantCulture);
            found = false;
            return 0;
        }

        internal static int GetSyncIntOutParameter(string parameter, IDbCommand command)
        {
            bool found;
            return SyncUtil.GetSyncIntOutParameter(parameter, command, out found);
        }

        internal static bool ParseTimestamp(object obj, out ulong timestamp)
        {
            timestamp = 0UL;
            if (obj is long || obj is int || (obj is ulong || obj is uint) || obj is Decimal)
            {
                timestamp = Convert.ToUInt64(obj, (IFormatProvider)NumberFormatInfo.InvariantInfo);
                return true;
            }
            else
            {
                string s = obj as string;
                if (s != null)
                    return ulong.TryParse(s, NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, out timestamp);
                byte[] numArray = obj as byte[];
                if (numArray == null)
                    return false;
                StringBuilder stringBuilder = new StringBuilder();
                for (int index = 0; index < numArray.Length; ++index)
                {
                    string str = numArray[index].ToString("X", (IFormatProvider)NumberFormatInfo.InvariantInfo);
                    stringBuilder.Append(str.Length == 1 ? "0" + str : str);
                }
                return ulong.TryParse(((object)stringBuilder).ToString(), NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, out timestamp);
            }
        }

        internal static bool ParseTimestampZeroIfNull(object obj, out ulong timestamp)
        {
            timestamp = 0UL;
            if (obj is DBNull)
                return true;
            else
                return SyncUtil.ParseTimestamp(obj, out timestamp);
        }

        internal static bool ParseKey(object obj, out uint key)
        {
            key = 0U;
            if (obj is int || obj is uint || obj is Decimal)
            {
                key = Convert.ToUInt32(obj, (IFormatProvider)NumberFormatInfo.InvariantInfo);
                return true;
            }
            else
            {
                string s = obj as string;
                if (s != null)
                    return uint.TryParse(s, NumberStyles.HexNumber, (IFormatProvider)CultureInfo.InvariantCulture.NumberFormat, out key);
                else
                    return false;
            }
        }

        internal static bool ParseKeyZeroIfNull(object obj, out uint key)
        {
            key = 0U;
            if (obj is DBNull)
                return true;
            else
                return SyncUtil.ParseKey(obj, out key);
        }

        //internal static long GetRowSizeFromReader(IDataReader reader, DbDataReaderHandler readerHandler)
        //{
        //    long num = 0L;
        //    for (int i = 0; i < reader.FieldCount; ++i)
        //    {
        //        if (!readerHandler.IsTombstone || readerHandler.IdAndMetadataColumns[i])
        //        {
        //            Type fieldType = reader.GetFieldType(i);
        //            if (reader.IsDBNull(i))
        //                num += 5L;
        //            else if (fieldType == typeof(Guid))
        //                num += 16L;
        //            else if (fieldType == typeof(byte[]))
        //                num += reader.GetBytes(i, 0L, (byte[])null, 0, 0);
        //            else if (fieldType == typeof(string))
        //                num += reader.GetChars(i, 0L, (char[])null, 0, 0) * 2L;
        //            else
        //                num += SyncUtil.GetSizeForType(fieldType);
        //        }
        //    }
        //    return num;
        //}

        internal static long GetRowSizeFromDataRow(DataRow row)
        {
            bool flag = false;
            if (row.RowState == DataRowState.Deleted)
            {
                row.RejectChanges();
                flag = true;
            }
            long num = 0L;
            foreach (object obj in row.ItemArray)
            {
                string s = obj as string;
                byte[] numArray = obj as byte[];
                if (obj is DBNull)
                    num += 5L;
                else if (obj is Guid)
                    num += 16L;
                else if (s != null)
                    num += (long)Encoding.Unicode.GetByteCount(s);
                else if (numArray != null)
                    num += (long)numArray.Length;
                else
                    num += SyncUtil.GetSizeForType(obj.GetType());
            }
            if (flag)
                row.Delete();
            return num;
        }

        internal static long GetRowSizeForObjectCollection(SyncUtil.ObjectCollection row)
        {
            long num = 0L;
            foreach (object obj in row.EnumColumns())
            {
                string s = obj as string;
                byte[] numArray = obj as byte[];
                if (obj == null)
                    ++num;
                else if (obj is Guid)
                    num += 16L;
                else if (s != null)
                    num += (long)Encoding.Unicode.GetByteCount(s);
                else if (numArray != null)
                    num += (long)numArray.Length;
                else
                    num += SyncUtil.GetSizeForType(obj.GetType());
            }
            return num;
        }

        internal static long GetSizeForType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    return 0L;
                case TypeCode.Object:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Double:
                case TypeCode.DateTime:
                    return 8L;
                case TypeCode.Boolean:
                case TypeCode.SByte:
                case TypeCode.Byte:
                    return 1L;
                case TypeCode.Char:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                    return 2L;
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                    return 4L;
                case TypeCode.Decimal:
                    return 16L;
                default:
                    return 0L;
            }
        }

        internal static bool TryGetReplicaKey(ReplicaKeyMap keyMap, SyncId syncId, out uint key)
        {
            bool flag = false;
            key = 0U;
            try
            {
                key = keyMap.LookupReplicaKey(syncId);
                flag = true;
            }
            catch (ReplicaNotFoundException ex)
            {
            }
            return flag;
        }

        internal static bool TryGetReplicaId(ReplicaKeyMap keyMap, uint key, out SyncId syncId)
        {
            bool flag = false;
            syncId = (SyncId)null;
            try
            {
                syncId = keyMap.LookupReplicaId(key);
                flag = true;
            }
            catch (ReplicaNotFoundException ex)
            {
            }
            return flag;
        }

        internal static bool VerifyKnowledgeKeyMapCompatibility(SyncKnowledge knowledgeA, SyncKnowledge knowledgeB)
        {
            bool flag = true;
            ReplicaKeyMap replicaKeyMap1 = knowledgeA.ReplicaKeyMap;
            ReplicaKeyMap replicaKeyMap2 = knowledgeB.ReplicaKeyMap;
            SyncId syncId1;
            SyncId syncId2;
            for (uint key = 0U; SyncUtil.TryGetReplicaId(replicaKeyMap1, key, out syncId1) && SyncUtil.TryGetReplicaId(replicaKeyMap2, key, out syncId2); ++key)
            {
                if (syncId1 != syncId2)
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }

        internal static int Compare(string strA, string strB)
        {
            return CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
        }

        internal static bool CompareInsensitiveInvariant(string strvalue, string strconst)
        {
            return 0 == CultureInfo.InvariantCulture.CompareInfo.Compare(strvalue, strconst, CompareOptions.IgnoreCase);
        }

        internal static int SrcCompare(string strA, string strB)
        {
            return !(strA == strB) ? 1 : 0;
        }

        internal static bool IsEqual(string strA, string strB)
        {
            return 0 == CultureInfo.CurrentCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth);
        }

        internal static bool IsEmpty(string str)
        {
            if (str != null)
                return 0 == str.Length;
            else
                return true;
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        internal static void BuildSchemaTableInfoTableNames(string[] columnNameArray)
        {
            Dictionary<string, int> hash = new Dictionary<string, int>(columnNameArray.Length);
            int val1 = columnNameArray.Length;
            for (int index = columnNameArray.Length - 1; 0 <= index; --index)
            {
                string str = columnNameArray[index];
                if (str != null && 0 < str.Length)
                {
                    string key = str.ToLower(CultureInfo.InvariantCulture);
                    int val2;
                    if (hash.TryGetValue(key, out val2))
                        val1 = Math.Min(val1, val2);
                    hash[key] = index;
                }
                else
                {
                    columnNameArray[index] = "";
                    val1 = index;
                }
            }
            int uniqueIndex = 1;
            for (int index1 = val1; index1 < columnNameArray.Length; ++index1)
            {
                string str = columnNameArray[index1];
                if (str.Length == 0)
                {
                    columnNameArray[index1] = "Column";
                    uniqueIndex = SyncUtil.GenerateUniqueName(hash, ref columnNameArray[index1], index1, uniqueIndex);
                }
                else
                {
                    string index2 = str.ToLower(CultureInfo.InvariantCulture);
                    if (index1 != hash[index2])
                        SyncUtil.GenerateUniqueName(hash, ref columnNameArray[index1], index1, 1);
                }
            }
        }

        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        private static int GenerateUniqueName(Dictionary<string, int> hash, ref string columnName, int index, int uniqueIndex)
        {
            string str;
            string key;
            while (true)
            {
                str = columnName + uniqueIndex.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                key = str.ToLower(CultureInfo.InvariantCulture);
                if (hash.ContainsKey(key))
                    ++uniqueIndex;
                else
                    break;
            }
            columnName = str;
            hash.Add(key, index);
            return uniqueIndex;
        }

        internal static string BuildQuotedString(string quotePrefix, string quoteSuffix, string unQuotedString)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!SyncUtil.IsEmpty(quotePrefix))
                stringBuilder.Append(quotePrefix);
            if (!SyncUtil.IsEmpty(quoteSuffix))
            {
                stringBuilder.Append(unQuotedString.Replace(quoteSuffix, quoteSuffix + quoteSuffix));
                stringBuilder.Append(quoteSuffix);
            }
            else
                stringBuilder.Append(unQuotedString);
            return ((object)stringBuilder).ToString();
        }

        internal static bool RemoveStringQuotes(string quotePrefix, string quoteSuffix, string quotedString, out string unquotedString)
        {
            int startIndex = quotePrefix != null ? quotePrefix.Length : 0;
            int num = quoteSuffix != null ? quoteSuffix.Length : 0;
            if (num + startIndex == 0)
            {
                unquotedString = quotedString;
                return true;
            }
            else if (quotedString == null)
            {
                unquotedString = quotedString;
                return false;
            }
            else
            {
                int length = quotedString.Length;
                if (length < startIndex + num)
                {
                    unquotedString = quotedString;
                    return false;
                }
                else if (startIndex > 0 && !quotedString.StartsWith(quotePrefix, StringComparison.Ordinal))
                {
                    unquotedString = quotedString;
                    return false;
                }
                else
                {
                    if (num > 0)
                    {
                        if (!quotedString.EndsWith(quoteSuffix, StringComparison.Ordinal))
                        {
                            unquotedString = quotedString;
                            return false;
                        }
                        else
                            unquotedString = quotedString.Substring(startIndex, length - (startIndex + num)).Replace(quoteSuffix + quoteSuffix, quoteSuffix);
                    }
                    else
                        unquotedString = quotedString.Substring(startIndex, length - startIndex);
                    return true;
                }
            }
        }

        internal static bool CompareColumnNames(string quotePrefix, string quoteSuffix, string col1, string col2)
        {
            string unquotedString1;
            SyncUtil.RemoveStringQuotes(quotePrefix, quoteSuffix, col1, out unquotedString1);
            string unquotedString2;
            SyncUtil.RemoveStringQuotes(quotePrefix, quoteSuffix, col2, out unquotedString2);
            return string.Compare(unquotedString1, unquotedString2, StringComparison.Ordinal) == 0;
        }

        internal interface ObjectCollection
        {
            IEnumerable<object> EnumColumns();
        }
    }
}