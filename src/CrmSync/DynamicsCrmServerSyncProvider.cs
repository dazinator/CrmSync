using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using CrmAdo;
using Microsoft.Synchronization.Data;
using Microsoft.Synchronization.Data.Server;

namespace CrmSync
{
    public class DynamicsCrmServerSyncProvider : DbServerSyncProvider
    {



        public const string EntityName = "new_synctest";
        //public static string[] SelectColumns =
        //    ("createdby,createdon,createdonbehalfby,entityimage,exchangerate," +
        //    "importsequencenumber,modifiedby,modifiedon,modifiedonbehalfby,new_contactlookup,new_currencycurrencyprecision," +
        //    "new_currencycurrencyprecision_base,new_currencyprecision0,new_currencyprecision0_base,new_currencyprecision1," +
        //    "new_currencyprecision1_base,new_currencyprecision2,new_currencyprecision2_base,new_currencyprecision3," +
        //    "new_currencyprecision3_base,new_currencyprecision4,new_currencyprecision4_base,new_currencypricingprecision," +
        //    "new_currencypricingprecision_base,new_dateandtime,new_dateonly,new_decimalprecision0,new_decimalprecision1," +
        //    "new_decimalprecision2,new_decimalprecision3,new_decimalprecision4,new_decimalprecision5,new_decimalprecision6," +
        //    "new_decimalprecision7,new_decimalprecision8,new_decimalprecision9,new_decimalprecision10,new_floatingpointprecision0," +
        //    "new_floatingpointprecision1,new_floatingpointprecision2,new_floatingpointprecision3,new_floatingpointprecision4," +
        //    "new_floatingpointprecision5,new_multiplelinesoftext,new_name,new_optionset,new_synctestid,new_twooptions," +
        //    "new_wholenumberduration,new_wholenumberlanguage,new_wholenumbernone,new_wholenumbertimezone,overriddencreatedon," +
        //    "ownerid,owningbusinessunit,owningteam,owninguser,statecode,statuscode,timezoneruleversionnumber,transactioncurrencyid," +
        //    "utcconversiontimezonecode,versionnumber").Split(',');

        public static string[] SelectColumns =
          ("createdby,createdonbehalfby," +
          "modifiedby,modifiedonbehalfby,new_contactlookup," +
          "new_name,new_synctestid," +
          "ownerid,owningbusinessunit,owningteam,owninguser,transactioncurrencyid," +
          "versionnumber").Split(',');

        public static string[] InsertColumns;
        public static string[] UpdateColumns;


        //  public static string[] InsertColumns = 

        public static Dictionary<string, DbType> ColumnInfo = new Dictionary<string, DbType>();

        private void LoadColumnInfo()
        {

            // could get metadata from crm.//new_currencypricingprecision," +
           // ColumnInfo["createdon"] = DbType.DateTime;
          //  ColumnInfo["modifiedon"] = DbType.DateTime;
            ColumnInfo["createdby"] = DbType.Guid;
            ColumnInfo["createdonbehalfby"] = DbType.Guid;
            ColumnInfo["modifiedby"] = DbType.Guid;
            ColumnInfo["modifiedonbehalfby"] = DbType.Guid;
            ColumnInfo["new_contactlookup"] = DbType.Guid;
            //ColumnInfo["new_currencycurrencyprecision"] = DbType.Currency;
            //ColumnInfo["new_currencycurrencyprecision_base"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision0"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision0_base"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision1"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision1_base"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision2"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision2_base"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision3"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision3_base"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision4"] = DbType.Currency;
            //ColumnInfo["new_currencyprecision4_base"] = DbType.Currency;
            //ColumnInfo["new_currencypricingprecision"] = DbType.Currency;
            //ColumnInfo["new_currencypricingprecision_base"] = DbType.Currency;
            //ColumnInfo["new_dateandtime"] = DbType.DateTime;
            //ColumnInfo["new_dateonly"] = DbType.Date;
            //ColumnInfo["new_decimalprecision0"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision1"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision2"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision3"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision4"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision5"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision6"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision7"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision8"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision9"] = DbType.Decimal;
            //ColumnInfo["new_decimalprecision10"] = DbType.Decimal;
            //ColumnInfo["new_floatingpointprecision0"] = DbType.Double;
            //ColumnInfo["new_floatingpointprecision1"] = DbType.Double;
            //ColumnInfo["new_floatingpointprecision2"] = DbType.Double;
            //ColumnInfo["new_floatingpointprecision3"] = DbType.Double;
            //ColumnInfo["new_floatingpointprecision4"] = DbType.Double;
            //ColumnInfo["new_floatingpointprecision5"] = DbType.Double;
            //ColumnInfo["new_multiplelinesoftext"] = DbType.String;
            ColumnInfo["new_name"] = DbType.String;
           // ColumnInfo["new_optionset"] = DbType.Int32;
            ColumnInfo["new_synctestid"] = DbType.Guid;
          
           // ColumnInfo["new_twooptions"] = DbType.Boolean;
            //ColumnInfo["new_wholenumberduration"] = DbType.Int32;
            //ColumnInfo["new_wholenumberlanguage"] = DbType.Int32;
            //ColumnInfo["new_wholenumbernone"] = DbType.Int32;
            //ColumnInfo["new_wholenumbertimezone"] = DbType.Int32;

            ColumnInfo["ownerid"] = DbType.Guid;
            ColumnInfo["owningbusinessunit"] = DbType.Guid;
            ColumnInfo["owningteam"] = DbType.Guid;
            ColumnInfo["owninguser"] = DbType.Guid;
            ColumnInfo["transactioncurrencyid"] = DbType.Guid;
            ColumnInfo["versionnumber"] = DbType.Int64;

            string[] excludeNames = new string[] { "createdby", "createdonbehalfby", "modifiedby", "modifiedonbehalfby", "owningbusinessunit", "ownerid", "owningteam", "owninguser", "transactioncurrencyid", "versionnumber" };
           // var excludeForInsertAndUpdate = (from a in ColumnInfo where excludeNames.Contains(a.Key) select a).ToArray();
            InsertColumns = (from a in ColumnInfo where !excludeNames.Contains(a.Key) select a.Key).ToArray();
            UpdateColumns = (from a in ColumnInfo where !excludeNames.Contains(a.Key) select a.Key).ToArray();
        }

        public DynamicsCrmServerSyncProvider()
        {
            LoadColumnInfo();
            // new fields required against entities to be synchronised.
            // crmsync_updatedbyclientid
            // crmsync_insertedbyclientid

            // Those fields will allow a client to filter out change detections for records that it has inserted or updated itself during a last sync run.
            // In addition:-

            // plugin to be fire on delete of entities in crm that will:-
            //  create a "sync_tombstone" record:-
            //      entity name, 
            //      entity id, 
            //      sync_deletedbyclientid

            //  sync_deletedbyclientid is only set by a sync client that syncs a delete to CRM, so it can later filter out its own delete from change detection on the next sync.


            //Create a connection to the sample server database.
            var util = new Utility();
            var serverConn = new CrmDbConnection(Utility.ConnStr_DbServerSync);
            this.Connection = serverConn;

            DbCommand selectNewAnchorCommand = new AnchorDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);
            var anchorParam = selectNewAnchorCommand.CreateParameter();
            anchorParam.ParameterName = "@" + SyncSession.SyncNewReceivedAnchor;

            var entityName = EntityName;
            var sql = "select top 1 versionnumber from " + entityName + " order by versionnumber desc";

            selectNewAnchorCommand.CommandText = sql;
            selectNewAnchorCommand.Parameters.Add(anchorParam);
            anchorParam.Direction = ParameterDirection.Output;
            selectNewAnchorCommand.Connection = serverConn;
            this.SelectNewAnchorCommand = selectNewAnchorCommand;

            //Create the SyncAdapter.
            var customerSyncAdapter = new SyncAdapter(entityName);
            var customerIncrInserts = new SelectIncrementalChangesDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);
            customerIncrInserts.CommandText =
                "SELECT " +
                string.Join(",", SelectColumns) +
                " FROM " + entityName +
                " WHERE (versionnumber > @sync_last_received_anchor " +
                "AND versionnumber <= @sync_new_received_anchor " +
                ")";
            //  "AND crmsync_insertedbyclientid <> @sync_client_id)";

            var lastAnchorParam = customerIncrInserts.CreateParameter();
            lastAnchorParam.ParameterName = "@" + SyncSession.SyncLastReceivedAnchor;
            lastAnchorParam.DbType = DbType.Int64;
            customerIncrInserts.Parameters.Add(lastAnchorParam);

            var thisAnchorParam = customerIncrInserts.CreateParameter();
            thisAnchorParam.ParameterName = "@" + SyncSession.SyncNewReceivedAnchor;
            thisAnchorParam.DbType = DbType.Int64;
            customerIncrInserts.Parameters.Add(thisAnchorParam);

            var clientIdParam = customerIncrInserts.CreateParameter();
            clientIdParam.ParameterName = "@" + SyncSession.SyncClientId;
            clientIdParam.DbType = DbType.Guid;
            customerIncrInserts.Parameters.Add(clientIdParam);

            customerIncrInserts.Connection = serverConn;
            customerSyncAdapter.SelectIncrementalInsertsCommand = customerIncrInserts;


            //Apply inserts to the server.
            string[] insertParamNames = (from a in InsertColumns select "@" + a).ToArray();

            DbCommand contactInserts = new InsertEntityDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);
            contactInserts.CommandText =
                "INSERT INTO " + EntityName
            + " (" + string.Join(",", InsertColumns) + " ) " +
                "VALUES (" + string.Join(",", insertParamNames) + ")";

            foreach (var insertColumn in InsertColumns)
            {
                AddParameter(contactInserts, insertColumn, ColumnInfo[insertColumn]);
            }

            AddParameter(contactInserts, SyncSession.SyncClientId, DbType.Guid);
            var param = AddParameter(contactInserts, SyncSession.SyncRowCount, DbType.Int32);
            param.Direction = ParameterDirection.Output;

            contactInserts.Connection = serverConn;
            customerSyncAdapter.InsertCommand = contactInserts;

            //Select updates from the server.
            var customerIncrUpdates = new SelectIncrementalChangesDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);
            customerIncrUpdates.CommandText =
                "SELECT " +
                string.Join(",", SelectColumns) +
                " FROM " + entityName +
                " WHERE (versionnumber > @sync_last_received_anchor " +
                "AND versionnumber <= @sync_new_received_anchor " +
               ")";


            AddParameter(customerIncrUpdates, SyncSession.SyncLastReceivedAnchor, DbType.Int64);
            AddParameter(customerIncrUpdates, SyncSession.SyncNewReceivedAnchor, DbType.Int64);
            AddParameter(customerIncrUpdates, SyncSession.SyncClientId, DbType.Guid);
            customerIncrUpdates.Connection = serverConn;
            customerSyncAdapter.SelectIncrementalUpdatesCommand = customerIncrUpdates;


            var customerUpdates = new UpdateEntityDbCommandAdapter(serverConn.CreateCommand() as CrmDbCommand);
            string[] setColumns = (from c in UpdateColumns select c + "=" + "@" + c).ToArray();
            var idColumn = EntityName + "id";
            customerUpdates.CommandText =
                "UPDATE " + EntityName + " SET " +
               string.Join(",", setColumns) +
                " WHERE (" + idColumn + " = @" + idColumn + ")";

            foreach (var col in UpdateColumns)
            {
                AddParameter(contactInserts, col, ColumnInfo[col]);
            }

            AddParameter(customerUpdates, SyncSession.SyncClientId, DbType.Guid);
            AddParameter(customerUpdates, SyncSession.SyncForceWrite, DbType.Boolean);
            AddParameter(customerUpdates, SyncSession.SyncLastReceivedAnchor, DbType.Int64);
            AddParameter(customerUpdates, SyncSession.SyncRowCount, DbType.Int32);

            customerUpdates.Connection = serverConn;
            customerSyncAdapter.UpdateCommand = customerUpdates;


            ////Select deletes from the server.
            //DbCommand customerIncrDeletes = serverConn.CreateCommand();
            //customerIncrDeletes.CommandText =
            //    "SELECT contactid, firstname, lastname " +
            //    "FROM contactSyncTombstone " +
            //    "WHERE (@sync_initialized = 1 " +
            //    "AND DeleteTimestamp > @sync_last_received_anchor " +
            //    "AND DeleteTimestamp <= @sync_new_received_anchor " +
            //    "AND DeleteId <> @sync_client_id)";


            ////"SELECT CustomerId, CustomerName, SalesPerson, CustomerType " +
            ////  "FROM Sales.Customer_Tombstone " +
            ////  "WHERE (@sync_initialized = 1 " +
            ////  "AND DeleteTimestamp > @sync_last_received_anchor " +
            ////  "AND DeleteTimestamp <= @sync_new_received_anchor " +
            ////  "AND DeleteId <> @sync_client_id)";


            //AddParameter(customerIncrDeletes, SyncSession.SyncInitialized, DbType.Boolean);
            //AddParameter(customerIncrDeletes, SyncSession.SyncLastReceivedAnchor, DbType.Int64);
            //AddParameter(customerIncrDeletes, SyncSession.SyncNewReceivedAnchor, DbType.Int64);
            //AddParameter(customerIncrDeletes, SyncSession.SyncClientId, DbType.Guid);

            //customerIncrDeletes.Connection = serverConn;
            //customerSyncAdapter.SelectIncrementalDeletesCommand = customerIncrDeletes;

            ////DT: Might have to put this into a plugin..
            ////Apply deletes to the server.            
            //DbCommand customerDeletes = serverConn.CreateCommand();
            //customerDeletes.CommandText =
            //    "DELETE FROM Sales.Customer " +
            //    "WHERE (CustomerId = @CustomerId) " +
            //    "AND (@sync_force_write = 1 " +
            //    "OR (UpdateTimestamp <= @sync_last_received_anchor " + // hasnt been updated since
            //    "OR UpdateId = @sync_client_id)) " + // or could have been updated since but if it was us then thats fine we can still delete
            //    "SET @sync_row_count = @@rowcount " + // informs sync fx of the number of rows effected = should be 1
            //    "IF (@sync_row_count > 0)  BEGIN " + // as long as one record was deleted then we have to store the tombstone record - this could be done by a plugin..
            //    "UPDATE Sales.Customer_Tombstone " +
            //    "SET DeleteId = @sync_client_id " +
            //    "WHERE (CustomerId = @CustomerId) " +
            //    "END";

            //AddParameter(customerDeletes, "CustomerId", DbType.Guid);
            //AddParameter(customerDeletes, SyncSession.SyncForceWrite, DbType.Boolean);
            //AddParameter(customerDeletes, SyncSession.SyncLastReceivedAnchor, DbType.Int64);
            //AddParameter(customerDeletes, SyncSession.SyncClientId, DbType.Guid);
            //AddParameter(customerDeletes, SyncSession.SyncRowCount, DbType.Int32);

            //customerDeletes.Connection = serverConn;
            //customerSyncAdapter.DeleteCommand = customerDeletes;

            //Add the SyncAdapter to the server synchronization provider.
            this.SyncAdapters.Add(customerSyncAdapter);

        }

        private static DbParameter AddParameter(DbCommand command, string syncParamater, DbType dbType)
        {
            var par = command.CreateParameter();
            par.ParameterName = "@" + syncParamater;
            par.DbType = dbType;
            command.Parameters.Add(par);
            return par;
        }
    }
}