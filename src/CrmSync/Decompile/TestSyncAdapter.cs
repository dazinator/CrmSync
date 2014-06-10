using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Synchronization.Data.Server;

namespace CrmSync
{
    /// <summary>
    /// Represents a set of data commands that are used to obtain schema information and to retrieve and apply changes at the server database.
    /// </summary>
    public class TestSyncAdapter
    {
        private SyncAdapterCollection _parent;
        private string _tableName;
        private string _description;
        private IDbCommand _deleteCommand;
        private IDbCommand _insertCommand;
        private IDbCommand _updateCommand;
        private IDbCommand _incInsertsCommand;
        private IDbCommand _incUpdatesCommand;
        private IDbCommand _incDeletesCommand;
        private IDbCommand _updateConflictCommand;
        private IDbCommand _deleteConflictCommand;
        private SyncColumnMappingCollection _columnMappings;

        /// <summary>
        /// Gets a collection of <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> objects for the table. These objects map columns in a server table to the corresponding columns in a client table.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/> object for the table. The <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> objects in the collection map columns in a server table to the corresponding columns in a client table.
        /// </returns>
        public SyncColumnMappingCollection ColumnMappings
        {
            get
            {
                return this._columnMappings;
            }
        }

        /// <summary>
        /// Gets or sets the name of the table at the server for which to create the <see cref="T:Microsoft.Synchronization.Data.Server.SyncAdapter"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The name of the table at the server for which to create the <see cref="T:Microsoft.Synchronization.Data.Server.SyncAdapter"/>.
        /// </returns>
        public string TableName
        {
            get
            {
                return this._tableName;
            }
            set
            {
                this._tableName = value;
            }
        }

        /// <summary>
        /// Gets or sets a description for the synchronization adapter.
        /// </summary>
        /// 
        /// <returns>
        /// The description of the synchronization adapter.
        /// </returns>
        public string Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to insert data into the server database.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand InsertCommand
        {
            get
            {
                return this._insertCommand;
            }
            set
            {
                this._insertCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to update data in the server database.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand UpdateCommand
        {
            get
            {
                return this._updateCommand;
            }
            set
            {
                this._updateCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to delete data from the server database.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand DeleteCommand
        {
            get
            {
                return this._deleteCommand;
            }
            set
            {
                this._deleteCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to retrieve inserts made in the server database since the last synchronization.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand SelectIncrementalInsertsCommand
        {
            get
            {
                return this._incInsertsCommand;
            }
            set
            {
                this._incInsertsCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to retrieve updates made in the server database since the last synchronization.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand SelectIncrementalUpdatesCommand
        {
            get
            {
                return this._incUpdatesCommand;
            }
            set
            {
                this._incUpdatesCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to retrieve deletes made in the server database since the last synchronization.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand SelectIncrementalDeletesCommand
        {
            get
            {
                return this._incDeletesCommand;
            }
            set
            {
                this._incDeletesCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to identify updated rows that conflict with other changes.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand SelectConflictUpdatedRowsCommand
        {
            get
            {
                return this._updateConflictCommand;
            }
            set
            {
                this._updateConflictCommand = value;
            }
        }

        /// <summary>
        /// Gets or sets the query or stored procedure that is used to identify deleted rows that conflict with other changes.
        /// </summary>
        /// 
        /// <returns>
        /// An <see cref="T:System.Data.IDbCommand"/> object that contains a query or stored procedure.
        /// </returns>
        public IDbCommand SelectConflictDeletedRowsCommand
        {
            get
            {
                return this._deleteConflictCommand;
            }
            set
            {
                this._deleteConflictCommand = value;
            }
        }

        internal SyncAdapterCollection Parent
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Synchronization.Data.Server.SyncAdapter"/> class by using default values.
        /// </summary>
        public TestSyncAdapter()
        {
            this._columnMappings = new SyncColumnMappingCollection();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Synchronization.Data.Server.SyncAdapter"/> class by using a table name parameter.
        /// </summary>
        /// <param name="tableName">The name of the table in the server database.</param>
        public TestSyncAdapter(string tableName)
        {
            this._tableName = tableName;
            this._columnMappings = new SyncColumnMappingCollection();
        }

        /// <summary>
        /// Gets the client column name that corresponds to the specified server column name.
        /// </summary>
        /// 
        /// <returns>
        /// The client column name that corresponds to the specified server column name.
        /// </returns>
        /// <param name="serverColumn">The server column name for which you want to get the corresponding client column name.</param>
        public string GetClientColumnFromServerColumn(string serverColumn)
        {
            return this._columnMappings.GetClientColumnFromServerColumn(serverColumn);
        }

        internal void MapFromServerToClient(DataTable dataTable)
        {
            SyncExpt.CheckArgumentNull((object)dataTable, "dataTable");
            dataTable.TableName = this._tableName;
            if (this.ColumnMappings == null)
                return;
            foreach (DataColumn dataColumn in (InternalDataCollectionBase)dataTable.Columns)
            {
                int index = this.ColumnMappings.IndexOfServerColumn(dataColumn.ColumnName);
                if (index >= 0)
                    dataColumn.ColumnName = this.ColumnMappings[index].ClientColumn;
            }
        }

        internal void MapFromClientToServer(DataTable dataTable)
        {
            SyncExpt.CheckArgumentNull((object)dataTable, "dataTable");
            if (this.ColumnMappings == null)
                return;
            foreach (DataColumn dataColumn in (InternalDataCollectionBase)dataTable.Columns)
            {
                int index = this.ColumnMappings.IndexOfClientColumn(dataColumn.ColumnName);
                if (index >= 0)
                    dataColumn.ColumnName = this.ColumnMappings[index].ServerColumn;
            }
        }

        private static void SetDummySessionParameters(IDbCommand cmd)
        {
            if (cmd == null || cmd.Parameters == null || 0 > cmd.Parameters.Count)
                return;
            foreach (DbParameter dbParameter in (IEnumerable)cmd.Parameters)
                dbParameter.Value = (object)DBNull.Value;
        }

        /// <summary>
        /// Populates the schema information for the table that is specified in <see cref="P:Microsoft.Synchronization.Data.Server.SyncAdapter.TableName"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Data.DataTable"/> that contains the schema information.
        /// </returns>
        /// <param name="dataTable">The <see cref="T:System.Data.DataTable"/> to be populated with schema information.</param><param name="connection">An <see cref="T:System.Data.IDbConnection"/> object that is used to connect to the server database.</param><exception cref="T:System.ArgumentNullException"><paramref name="connection"/> is a null.</exception><exception cref="T:Microsoft.Synchronization.Data.SchemaException"><see cref="P:Microsoft.Synchronization.Data.Server.SyncAdapter.SelectIncrementalInsertsCommand"/> or <see cref="P:Microsoft.Synchronization.Data.Server.SyncAdapter.SelectIncrementalUpdatesCommand"/>  is a null, or the schema could not be retrieved.</exception>
        public DataTable FillSchema(DataTable dataTable, IDbConnection connection)
        {
            SyncExpt.CheckArgumentNull((object)connection, "connection");
            bool flag = SyncUtil.OpenConnection(connection);
            if (this.SelectIncrementalInsertsCommand == null && this.SelectIncrementalUpdatesCommand == null)
                throw SyncExpt.MissingSelectStatementError(this.TableName, "ServerSyncProvider", "http://www.microsoft.com/sql/");
            IDbCommand cmd = this.SelectIncrementalInsertsCommand == null ? this.SelectIncrementalUpdatesCommand : this.SelectIncrementalInsertsCommand;
            SetDummySessionParameters(cmd);
            cmd.Connection = connection;
           SyncDbAdapter syncDbAdapter = new SyncDbAdapter();
            syncDbAdapter.SelectCommand = (DbCommand)cmd;
            if (dataTable == null)
            {
                dataTable = new DataTable();
                dataTable.Locale = CultureInfo.InvariantCulture;
            }
            syncDbAdapter.FillSchema(dataTable, SchemaType.Source);
            IDataReader dataReader = cmd.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
            try
            {
                DataTable schemaTable = dataReader.GetSchemaTable();
                if (schemaTable == null)
                    throw SyncExpt.FillSchemaError(dataTable.TableName, "ServerSyncProvider", "http://www.microsoft.com/sql/", (Exception)null);
                if (schemaTable.Columns.Contains("DataTypeName"))
                {
                    foreach (DataRow dataRow in (InternalDataCollectionBase)schemaTable.Rows)
                    {
                        string name = (string)dataRow["ColumnName"];
                        if (dataTable.Columns.Contains(name))
                        {
                            DataColumn column = dataTable.Columns[name];
                            if (column != null)
                            {
                                object obj1 = dataRow["DataTypeName"];
                                if (obj1 != null)
                                    SetDataColumnExtendedProperty(column, "DataTypeName", (object)obj1.ToString());
                                if (column.DataType.Equals(Type.GetType("System.Decimal")))
                                {
                                    object obj2 = dataRow["NumericPrecision"];
                                    if (obj2 != null)
                                        SetDataColumnExtendedProperty(column, "NumericPrecision", obj2);
                                    object obj3 = dataRow["NumericScale"];
                                    if (obj3 != null)
                                        SetDataColumnExtendedProperty(column, "NumericScale", obj3);
                                }
                                object obj4 = dataRow["ColumnSize"];
                                if (obj4 != null)
                                {
                                    if ((int.MaxValue == (int)obj4 || 1073741823 == (int)obj4) && (column.DataType.Equals(Type.GetType("System.String")) || column.DataType.Equals(Type.GetType("System.Byte[]"))))
                                        SetDataColumnExtendedProperty(column, "ColumnLength", (object)-1);
                                    else
                                        SetDataColumnExtendedProperty(column, "ColumnLength", obj4);
                                }
                            }
                        }
                    }
                }
            }
            catch (DbException ex)
            {
                throw SyncExpt.FillSchemaError(dataTable.TableName, "ServerSyncProvider", "http://www.microsoft.com/sql/", (Exception)ex);
            }
            finally
            {
                dataReader.Close();
            }
            if (flag)
                connection.Close();
            this.MapFromServerToClient(dataTable);
            return dataTable;
        }

        private static void SetDataColumnExtendedProperty(DataColumn column, string property, object value)
        {
            if (column.ExtendedProperties[(object)property] == null)
                column.ExtendedProperties.Add((object)property, value);
            else
                column.ExtendedProperties[(object)property] = value;
        }

        /// <summary>
        /// Returns a string that represents the <see cref="T:Microsoft.Synchronization.Data.Server.SyncAdapter"/> object.
        /// </summary>
        /// 
        /// <returns>
        /// A string that represents the <see cref="T:Microsoft.Synchronization.Data.Server.SyncAdapter"/> object.
        /// </returns>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.ToString()).Append(" - ").Append(this.TableName);
            return ((object)stringBuilder).ToString();
        }
    }
}
