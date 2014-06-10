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
    public class UpdateEntityDbCommandAdapter : CrmDbCommand
    {
        // private List<string> _Log = new List<string>();
        private int _TotalUpdates = 0;

        private CrmDbCommand _WrappedCommand;

        public UpdateEntityDbCommandAdapter(CrmDbCommand wrappedCommand)
        {
            _WrappedCommand = wrappedCommand;
        }

        public override int ExecuteNonQuery()
        {
            Debug.WriteLine("Execute non query " + DateTime.Now + " for command text: " + this.CommandText);
            Execute();
            return 1;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        public override object ExecuteScalar()
        {
            Debug.WriteLine("Execute Scalar " + DateTime.Now + " for command text: " + this.CommandText);
            return Execute();
        }

        protected object Execute()
        {
            //todo: 
            // if sync force write then allways update..
            var forceParam = this.Parameters["@" + SyncSession.SyncForceWrite];
            var force = (bool)forceParam.Value;

            var lastAnchorParam = this.Parameters["@" + SyncSession.SyncLastReceivedAnchor];
            var lastAnchor = (long)lastAnchorParam.Value;

            var newAnchorParam = this.Parameters["@" + SyncSession.SyncNewReceivedAnchor];
            var newAnchor = (long)newAnchorParam.Value;

            var syncClientIdParam = this.Parameters["@" + SyncSession.SyncClientId];
            var syncClientId = newAnchorParam.Value;

            // if forced, or if versionnumber of record is less than or equal to the lastAnchor value, 
            // or if the crmsync_updatedbyclientid of the record is the same as this sync client id,
            // then we can update the record.

            // TODO May need to put this all into a crm plugin so that it can lock the record whilt it does the update?
            if (force)
            {
                // 
            }

            // Until above implemented then we allways just update the record.

            var param = this.Parameters["@" + SyncSession.SyncRowCount];

#if DEBUG
            Console.WriteLine("Updating entity in CRM.");
#endif

            //_WrappedCommand.CommandText = this.CommandText;
            var rowCount = _WrappedCommand.ExecuteNonQuery();
            Debug.WriteLine("update row count is " + rowCount);

#if DEBUG
            Console.WriteLine("row count was " + rowCount);
#endif
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