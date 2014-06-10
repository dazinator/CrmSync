using System;
using System.Data;
using System.Data.Common;

namespace CrmSync
{
    internal class SyncDbAdapter : DbDataAdapter
    {
        internal int FillFromReader(DataTable dataTable, IDataReader dataReader)
        {
            return base.Fill(dataTable, dataReader);
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow a, IDbCommand b, StatementType c, DataTableMapping d)
        {
            return (RowUpdatedEventArgs)new EventArgs();
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow a, IDbCommand b, StatementType c, DataTableMapping d)
        {
            return (RowUpdatingEventArgs)new EventArgs();
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
        }
    }
}