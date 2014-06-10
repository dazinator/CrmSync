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
    /// Adaptor for CrmDbCommand that will enable it to cater for Sync Anchor commands.
    /// </summary>
    public class AnchorDbCommandAdapter : CrmDbCommand
    {
        private List<string> _Log = new List<string>();

        private CrmDbCommand _WrappedCommand;

        public AnchorDbCommandAdapter(CrmDbCommand wrappedCommand)
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
            var param = this.Parameters["@" + SyncSession.SyncNewReceivedAnchor];
            // todo should be able to select max versionnumber from contact..
            // _WrappedCommand.CommandText = this.CommandText;

            var lastrowversion = _WrappedCommand.ExecuteScalar();
            Debug.WriteLine("last versionnumber is " + lastrowversion);

#if DEBUG
            Console.WriteLine("Get new anchor value: " + lastrowversion);
#endif

            if (lastrowversion == DBNull.Value || lastrowversion == null)
            {
                param.Value = 0L;
            }
            else
            {
                param.Value = (long)lastrowversion;
            }

            return lastrowversion;
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