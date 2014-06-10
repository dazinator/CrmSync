using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CrmSync
{
    /// <summary>
    /// Contains a collection of <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> objects.
    /// </summary>
    public class SyncColumnMappingCollection : Collection<SyncColumnMapping>
    {
        internal string GetClientColumnFromServerColumn(string serverColumn)
        {
            int index = this.IndexOfServerColumn(serverColumn);
            if (index > -1)
                return this.Items[index].ClientColumn;
            else
                return (string)null;
        }

        /// <summary>
        /// Adds a <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object to the end of the collection when given a server column name and a client column name.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object that represents the mapping between the columns specified in <paramref name="serverColumn"/> and <paramref name="clientColumn"/>.
        /// </returns>
        /// <param name="serverColumn">The name of the column at the server to add to the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/>.</param><param name="clientColumn">The name of the column at the client to add to the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/>.</param>
        public SyncColumnMapping Add(string serverColumn, string clientColumn)
        {
            SyncColumnMapping syncColumnMapping = new SyncColumnMapping(serverColumn, clientColumn);
            base.Add(syncColumnMapping);
            return syncColumnMapping;
        }

        /// <summary>
        /// Searches for a <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object when given a column name. Returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// 
        /// <returns>
        /// The index position of <paramref name="serverColumn"/> if that string is found. Returns -1 if the string is not found.
        /// </returns>
        /// <param name="serverColumn">The name of the column at the server for which to get the index in the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/>.</param>
        public int IndexOfServerColumn(string serverColumn)
        {
            if (!SyncUtil.IsEmpty(serverColumn))
            {
                int count = this.Count;
                for (int index = 0; index < count; ++index)
                {
                    if (serverColumn == this.Items[index].ServerColumn)
                        return index;
                }
            }
            return -1;
        }

        /// <summary>
        /// Searches for a <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object when given a column name, and returns the zero-based index of the first occurrence within the entire collection.
        /// </summary>
        /// 
        /// <returns>
        /// The index position of <paramref name="clientColumn"/> if that string is found; otherwise -1 if it is not.
        /// </returns>
        /// <param name="clientColumn">The name of the column at the client for which to get the index in the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/>.</param>
        public int IndexOfClientColumn(string clientColumn)
        {
            if (!SyncUtil.IsEmpty(clientColumn))
            {
                int count = this.Count;
                for (int index = 0; index < count; ++index)
                {
                    if (clientColumn == this.Items[index].ClientColumn)
                        return index;
                }
            }
            return -1;
        }

        internal void Validate(int index, SyncColumnMapping item)
        {
            if (item == null)
                throw SyncExpt.ArgumentNull("item");
            if (item.Parent != null && this != item.Parent)
                throw SyncExpt.Argument("", "item.Parent");
            string serverColumn1 = item.ServerColumn;
            if (SyncUtil.IsEmpty(serverColumn1))
            {
                index = 1;
                string serverColumn2;
                do
                {
                    serverColumn2 = "ServerColumn" + index.ToString((IFormatProvider)CultureInfo.InvariantCulture);
                    ++index;
                }
                while (-1 != this.IndexOfServerColumn(serverColumn2));
                item.ServerColumn = serverColumn2;
            }
            else
                this.ValidateServerColumn(index, serverColumn1);
            string clientColumn = item.ClientColumn;
            this.ValidateClientColumn(index, clientColumn);
        }

        internal void ValidateServerColumn(int index, string value)
        {
            int num = this.IndexOfServerColumn(value);
            if (-1 != num && index != num)
                throw SyncExpt.Argument("not unique");
        }

        internal void ValidateClientColumn(int index, string value)
        {
            int num = this.IndexOfClientColumn(value);
            if (-1 != num && index != num)
                throw SyncExpt.Argument("not unique");
        }

        /// <summary>
        /// Inserts a <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object into the collection at the specified index.
        /// </summary>
        /// <param name="index">The position in the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/> at which to insert the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object.</param><param name="item">The <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object to insert.</param><exception cref="T:System.ArgumentException"><paramref name="item"/> already exists in this <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/> object.</exception>
        protected override void InsertItem(int index, SyncColumnMapping item)
        {
            this.Validate(index, item);
            item.Parent = this;
            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object at the specified index.
        /// </summary>
        /// <param name="index">The position in the <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/> at which to replace the existing <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object with a new one.</param><param name="item">The new <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMapping"/> object to insert.</param><exception cref="T:System.ArgumentException"><paramref name="item"/> already exists in this <see cref="T:Microsoft.Synchronization.Data.Server.SyncColumnMappingCollection"/> object.</exception>
        protected override void SetItem(int index, SyncColumnMapping item)
        {
            this.Validate(index, item);
            item.Parent = this;
            base.SetItem(index, item);
        }
    }
}