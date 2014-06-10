namespace CrmSync
{
    /// <summary>
    /// Maps a column in a server table to the corresponding column in a client table.
    /// </summary>
    public class SyncColumnMapping
    {
        private string _serverColumn;
        private string _clientColumn;
        private SyncColumnMappingCollection _parent;

        /// <summary>
        /// Gets or sets the name of the client column.
        /// </summary>
        /// 
        /// <returns>
        /// The name of the column in the client database.
        /// </returns>
        public string ClientColumn
        {
            get
            {
                return this._clientColumn ?? "";
            }
            set
            {
                this._clientColumn = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the server column.
        /// </summary>
        /// 
        /// <returns>
        /// The name of the column in the server database.
        /// </returns>
        /// <exception cref="T:System.ArgumentException"><paramref name="value"/> is used by another <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object in the parent <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/> object.</exception>
        public string ServerColumn
        {
            get
            {
                return this._serverColumn ?? "";
            }
            set
            {
                if (this._parent != null && !SyncUtil.IsEqual(this._serverColumn, value))
                    this._parent.ValidateServerColumn(-1, value);
                this._serverColumn = value;
            }
        }

        internal SyncColumnMappingCollection Parent
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
        /// Initializes a new instance of the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> class by using default values.
        /// </summary>
        public SyncColumnMapping()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> class by using server column and client column parameters.
        /// </summary>
        /// <param name="serverColumn">The name of the column in the server database.</param><param name="clientColumn">The name of the column in the client database.</param>
        public SyncColumnMapping(string serverColumn, string clientColumn)
        {
            this.ServerColumn = serverColumn;
            this.ClientColumn = clientColumn;
        }

        /// <summary>
        /// Returns a string that represents the column mapping in the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/>.
        /// </summary>
        /// 
        /// <returns>
        /// A string that represents the column mapping in the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/>.
        /// </returns>
        public override string ToString()
        {
            return this.ServerColumn + "-" + this.ClientColumn;
        }
    }
}