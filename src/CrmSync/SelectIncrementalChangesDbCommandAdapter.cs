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
    public class SelectIncrementalChangesDbCommandAdapter : CrmDbCommand
    {
        // private List<string> _Log = new List<string>();

        private CrmDbCommand _WrappedCommand;

        //  private DbDataReader _CachedResults = null;

        // private bool _HasExecuted = false;


        public SelectIncrementalChangesDbCommandAdapter(CrmDbCommand wrappedCommand)
        {
            _WrappedCommand = wrappedCommand;
        }

        public override int ExecuteNonQuery()
        {
            Debug.WriteLine("Execute non query " + DateTime.Now + " for command text: " + this.CommandText);
            PreExecuteCheck();
#if DEBUG
            Console.WriteLine("Selecting incremental changes between " + this.Parameters[0].Value + " and " + this.Parameters[1].Value);
#endif
            return _WrappedCommand.ExecuteNonQuery();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            Debug.WriteLine("Execute non query " + DateTime.Now + " for command text: " + this.CommandText);
            PreExecuteCheck();
#if DEBUG
            Console.WriteLine("Selecting incremental changes between " + this.Parameters[0].Value + " and " + this.Parameters[1].Value);
#endif

            // On first execution, we actually get incremental inserts and updates in one.
            // but we then split these results out, and just return the incremental inserts.
            // We cache the "updates" until the next time we are called - and then we return the updates.
            // The reason for this, is because there is no easy way in dynamics for asking for just records that have been created and not updated,
            // the only way realy is to pull back the records first, then compare the createdon and modifiedon dates. If the dates are the same its a create,
            // otherwise its an update.
            // if (!_HasExecuted)
            // {
            var reader = _WrappedCommand.ExecuteReader(behavior) as CrmDbDataReader;
            return reader;
            // 

            //  } else




        }

        public override object ExecuteScalar()
        {
            Debug.WriteLine("Execute Scalar " + DateTime.Now + " for command text: " + this.CommandText);
            PreExecuteCheck();
            return _WrappedCommand.ExecuteScalar();
        }

        protected void PreExecuteCheck()
        {
            // if last anchor is currently dbnull (which it will be on very first sync) then change it to 0;
            var param = this.Parameters["@" + SyncSession.SyncLastReceivedAnchor];
            if (param != null)
            {
                if (param.Value == DBNull.Value)
                {
                    param.Value = 0L;
                }
                else if (param.Value is int)
                {
                    param.Value = System.Convert.ToInt64(param.Value);
                }
            }
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