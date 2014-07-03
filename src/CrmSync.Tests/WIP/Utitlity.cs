using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CrmAdo;
using CrmSync.Tests;

namespace CrmSync
{
    public class Utility
    {


        public static Guid CrmSystemUserId = Guid.Parse("ac229fe3-40b1-e311-9caa-d89d6764506c");
        public static Guid CrmTransactionCurrency = Guid.Parse("ffb0803a-40b1-e311-9351-6c3be5be9f98");
        public static Guid CrmContactId = Guid.Parse("01fd8460-eee4-e311-a935-6c3be5be5ec4");
        
     
        // ----------  BEGIN CODE RELATED TO SQL SERVER COMPACT --------- //

        public static void DeleteAndRecreateCompactDatabase(string sqlCeConnString, bool recreateDatabase)
        {
            using (SqlCeConnection clientConn = new SqlCeConnection(sqlCeConnString))
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                string filePath = clientConn.Database;
                if (dataDirectory != null)
                {
                    if (!dataDirectory.EndsWith(@"\"))
                    {
                        dataDirectory = dataDirectory + @"\";
                    }
                    filePath = clientConn.Database.Replace("|DataDirectory|", dataDirectory);
                }
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            if (recreateDatabase == true)
            {
                SqlCeEngine sqlCeEngine = new SqlCeEngine(sqlCeConnString);
                sqlCeEngine.CreateDatabase();
            }

        }

        // ----------  END CODE RELATED TO SQL SERVER COMPACT --------- //
        

        /* ----------  BEGIN CODE FOR DBSERVERSYNCPROVIDER AND --------- //
           ----------      SQLCECLIENTSYNCPROVIDER SAMPLES     --------- */

        public static string BuildSqlValuesClause(List<ColumnInfo> columns, Dictionary<string, string> specificValues)
        {
            var builder = new StringBuilder(String.Empty);
            var rand = new Random();

            foreach (var column in columns)
            {

                if (specificValues.ContainsKey(column.AttributeName))
                {
                    builder.Append(specificValues[column.AttributeName]);
                }
                else
                {
                    switch (column.Type)
                    {
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.String:
                        case DbType.StringFixedLength:
                            builder.Append("'");
                            builder.Append("randomstringval");
                            builder.Append(Guid.NewGuid().ToString());
                            builder.Append("'");
                            break;
                        case DbType.Boolean:
                            int randomInt = rand.Next(0, 1);
                            // bool randomBool = randomInt == 1;
                            builder.Append(randomInt.ToString());
                            break;
                        case DbType.Currency:

                            builder.Append("10");
                            break;

                        case DbType.Date:
                            builder.Append("'");
                            builder.Append(DateTime.UtcNow.Date.ToString(CultureInfo.InvariantCulture));
                            builder.Append("'");
                            break;
                        case DbType.DateTime:
                            builder.Append("'");
                            builder.Append(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                            builder.Append("'");
                            break;
                        case DbType.Decimal:
                            builder.Append("11");
                            break;

                        case DbType.Double:
                            builder.Append("12");
                            break;

                        case DbType.Guid:
                            builder.Append("'");
                            builder.Append(Guid.NewGuid().ToString());
                            builder.Append("'");
                            break;
                        case DbType.Int16:
                            builder.Append(rand.Next(0, Int16.MaxValue));
                            break;
                        case DbType.Int32:
                            builder.Append(rand.Next(0, Int32.MaxValue));
                            break;

                        case DbType.Int64:
                            builder.Append((long)LongRandom(0, Int64.MaxValue, rand));
                            break;


                        case DbType.DateTimeOffset:
                        case DbType.DateTime2:
                        case DbType.Byte:
                        case DbType.Binary:
                            throw new NotSupportedException("column type of : " + column.Type.ToString() + "is not supported.");
                        default:
                            throw new NotSupportedException("column type of : " + column.Type.ToString() + "is not supported.");

                    }
                }


                builder.Append(",");
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        private static long LongRandom(long min, long max, Random rand)
        {
            byte[] buf = new byte[8];
            rand.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);
            return (Math.Abs(longRand % (max - min)) + min);
        }

        //public static void MakeDataChangesOnServer(string tableName)
        //{
        //    return;
        //    int rowCount = 0;

        //    using (CrmDbConnection serverConn = new CrmDbConnection(ConnStr_DbServerSync))
        //    {

        //        serverConn.Open();
        //        if (tableName == TestDynamicsCrmServerSyncProvider.EntityName)
        //        {
        //            // An insert..

        //            var valuesForInsert = new Dictionary<string, string>();
        //            //                                       21476b89-41b1-e311-9351-6c3be5be9f98
        //            valuesForInsert["new_contactlookup"] = "'21476b89-41b1-e311-9351-6c3be5be9f98'";
        //            valuesForInsert["new_optionset"] = "100000002";
        //            valuesForInsert["new_wholenumberlanguage"] = "1033";
        //            valuesForInsert["new_wholenumbertimezone"] = "85";
        //            valuesForInsert["new_wholenumberduration"] = "55";
        //            // valuesForInsert["new_synctestid"] = "'af42495d-642d-480a-8fdc-24ec328d294e'";

        //            var valuesClause = GetValuesClauseForInsert(TestDynamicsCrmServerSyncProvider.ColumnInfo, valuesForInsert);


        //            using (var command = serverConn.CreateCommand())
        //            {
        //                command.CommandText =
        //                 "INSERT INTO " + TestDynamicsCrmServerSyncProvider.EntityName + " ("
        //                + String.Join(",", TestDynamicsCrmServerSyncProvider.InsertColumns) +
        //                   ") " +
        //                 "VALUES (" + valuesClause + ")";
        //                rowCount = command.ExecuteNonQuery();
        //            }

        //            // An update..
        //            //using (var command = serverConn.CreateCommand())
        //            //{
        //            //    command.CommandText =
        //            //     "UPDATE contact " +
        //            //     "SET  firstname = 'James' " +
        //            //     "WHERE firstname = 'Tandem Bicycle Store' "
        //            //       var result = command.ExecuteNonQuery();
        //            //}
        //            // A delete..
        //            //using (var command = serverConn.CreateCommand())
        //            //{
        //            //    command.CommandText =//   
        //            //       var result = command.ExecuteNonQuery();
        //            //}

        //        }

        //        serverConn.Close();
        //    }

        //    Console.WriteLine("Rows inserted, updated, or deleted at the server: " + rowCount);
        //}

        //Revert changes that were made during synchronization.
        //public static void CleanUpServer()
        //{
        //    //using (SqlConnection serverConn = new SqlConnection(Utility.ConnStr_DbServerSync))
        //    //{
        //    //    SqlCommand sqlCommand = serverConn.CreateCommand();
        //    //    sqlCommand.CommandType = CommandType.StoredProcedure;
        //    //    sqlCommand.CommandText = "usp_InsertSampleData";

        //    //    serverConn.Open();
        //    //    sqlCommand.ExecuteNonQuery();
        //    //    serverConn.Close();
        //    //}
        //}



        //public static void MakeDataChangesOnClient(string tableName)
        //{
        //    int rowCount = 0;

        //    using (SqlCeConnection clientConn = new SqlCeConnection(Utility.ConnStr_SqlCeClientSync))
        //    {

        //        clientConn.Open();

        //        if (tableName == DynamicsCrmServerSyncProvider.EntityName)
        //        {
        //            using (var sqlCeCommand = clientConn.CreateCommand())
        //            {

        //                var valuesForInsert = new Dictionary<string, string>();
        //                valuesForInsert["new_contactlookup"] = "'" + CrmContactId.ToString() + "'";
        //                valuesForInsert["new_optionset"] = "100000002";

        //                //  valuesForInsert["createdby"] = "'" + CrmSystemUserId + "'";
        //                //   valuesForInsert["modifiedby"] = "'" + CrmSystemUserId + "'";
        //                //  valuesForInsert["createdonbehalfby"] = "'" + CrmSystemUserId + "'";
        //                //  valuesForInsert["modifiedonbehalfby"] = "'" + CrmSystemUserId + "'";
        //                //  valuesForInsert["ownerid"] = "'" + CrmSystemUserId + "'";
        //                //   valuesForInsert["transactioncurrencyid"] = "'" + CrmTransactionCurrency + "'";

        //                //valuesForInsert["new_wholenumberlanguage"] = "1033";
        //                // valuesForInsert["new_wholenumbertimezone"] = "85";
        //                // valuesForInsert["new_wholenumberduration"] = "55";
        //                // valuesForInsert["new_synctestid"] = "'ef42495d-642d-480a-8fdc-24ec328d294a'";
        //                var insertColumns = (from a in DynamicsCrmServerSyncProvider.ColumnInfo
        //                                     where DynamicsCrmServerSyncProvider.InsertColumns.Contains(a.Key)
        //                                     select a);

        //                var insertColumnsDictionary = new Dictionary<string, DbType>();
        //                foreach (var i in insertColumns)
        //                {
        //                    insertColumnsDictionary.Add(i.Key, i.Value);
        //                }


        //                var valuesClause = GetValuesClauseForInsert(insertColumnsDictionary, valuesForInsert);

        //                sqlCeCommand.CommandText =
        //                "INSERT INTO " + DynamicsCrmServerSyncProvider.EntityName + " ("
        //                + string.Join(",", DynamicsCrmServerSyncProvider.InsertColumns) +
        //                   ") " +
        //                 "VALUES (" + valuesClause + ")";
        //                rowCount = sqlCeCommand.ExecuteNonQuery();
        //            }
        //        }

        //        clientConn.Close();
        //    }

        //    Console.WriteLine("Rows inserted, updated, or deleted at the client: " + rowCount);
        //}

        /* ----------  END CODE FOR DBSERVERSYNCPROVIDER AND   --------- //
           ----------      SQLCECLIENTSYNCPROVIDER SAMPLES     --------- */
    }
}
