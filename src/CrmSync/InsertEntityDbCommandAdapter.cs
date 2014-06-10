using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using CrmAdo;
using Microsoft.Synchronization.Data;

namespace CrmSync
{
    /// <summary>
    /// Adaptor for CrmDbCommand that will enable it to cater for Sync insert commands by setting row count paramater.
    /// </summary>
    public class InsertEntityDbCommandAdapter : CrmDbCommand
    {
        // private List<string> _Log = new List<string>();
        private int _TotalUpdates = 0;

        private CrmDbCommand _WrappedCommand;

        public InsertEntityDbCommandAdapter(CrmDbCommand wrappedCommand)
        {
            _WrappedCommand = wrappedCommand;
        }

        public override int ExecuteNonQuery()
        {
            Debug.WriteLine("Execute non query " + DateTime.Now + " for command text: " + this.CommandText);

#if DEBUG
            Console.Write("Inserting entity into CRM. ");
#endif

            var rowCount = _WrappedCommand.ExecuteNonQuery();
            var param = this.Parameters["@" + SyncSession.SyncRowCount];
            Debug.WriteLine("insert row count is " + rowCount);
            param.Value = rowCount;
            _TotalUpdates += rowCount;
#if DEBUG
            Console.Write("insert row count: " + rowCount);
#endif
            return rowCount;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            Debug.WriteLine("Execute Scalar " + DateTime.Now + " for command text: " + this.CommandText);
            var rowCount = _WrappedCommand.ExecuteNonQuery();
            var param = this.Parameters["@" + SyncSession.SyncRowCount];
            Debug.WriteLine("insert row count is " + rowCount);
            param.Value = rowCount;
            _TotalUpdates += rowCount;
            return rowCount;
        }

        public override string CommandText
        {
            get
            {
                return _WrappedCommand.CommandText;
            }
            set
            {
                _WrappedCommand.CommandText = value;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _WrappedCommand.Parameters; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // also dispose of wrapped command.
            Debug.WriteLine("disposing of wrapped command");
            _WrappedCommand.Dispose();
        }
    }
}