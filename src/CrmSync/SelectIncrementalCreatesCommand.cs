using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using CrmAdo;
using Microsoft.Synchronization.Data;

namespace CrmSync
{

    /// <summary>
    /// Adaptor for CrmDbCommand that will enable it to cater selecting incremental changes.
    /// </summary>
    public class SelectIncrementalCreatesCommand : CrmDbCommand
    {

        public static string SyncClientId;

        private CrmDbCommand _WrappedCommand;

        private static object DefaultAnchorValue = System.Convert.ChangeType(0,
                                                                      Plugin.SyncColumnInfo
                                                                            .CreationVersionColumnType);


        public SelectIncrementalCreatesCommand(CrmDbCommand wrappedCommand)
        {
            _WrappedCommand = wrappedCommand;
        }

        public override int ExecuteNonQuery()
        {
            Debug.WriteLine("Selecting incremental creates @ " + DateTime.Now + " Command text: " + this.CommandText);
            PreExecuteCheck();
#if DEBUG
            Console.WriteLine("Selecting incremental creates between " + this.Parameters[0].Value + " and " + this.Parameters[1].Value);
#endif
            var results = _WrappedCommand.ExecuteNonQuery();
            return results;
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            // Debug.WriteLine("Execute non query " + DateTime.Now + " for command text: " + this.CommandText);
            PreExecuteCheck();
#if DEBUG
            Console.WriteLine("Selecting incremental creates between " + this.Parameters[0].Value + " and " + this.Parameters[1].Value);
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
            // Debug.WriteLine("Execute Scalar " + DateTime.Now + " for command text: " + this.CommandText);
            PreExecuteCheck();
            return _WrappedCommand.ExecuteScalar();
        }

        protected void PreExecuteCheck()
        {
            // if last anchor is currently dbnull (which it will be on very first sync) then change it to 0;
            // because our anchor values are long's but we aren't allowed to create "long" fields in dynamics, we have to
            // convert between the long value and a decimal as our custom field for creation version is a decimal.


            var clientParam = this.Parameters["@" + SyncSession.SyncClientId];
            if (clientParam != null)
            {
                if (clientParam.Value != null && clientParam.Value != DBNull.Value)
                {

                    ChangeSyncClientParameterType(clientParam);
                    SyncClientId = (String)clientParam.Value;
                }
            }


            var lastAnchorParam = this.Parameters["@" + SyncSession.SyncLastReceivedAnchor];
            ChangeAnchorParameterType(lastAnchorParam);

            // if the last anchor is zero this indicates that this is the first time we are synchronising.
            if (lastAnchorParam == null || lastAnchorParam.Value == DefaultAnchorValue)
            {
                FirstSync();
            }


            var newAnchorParam = this.Parameters["@" + SyncSession.SyncNewReceivedAnchor];
            ChangeAnchorParameterType(newAnchorParam);

        }

        protected virtual void FirstSync()
        {
            Debug.WriteLine("First Sync with Server.");
            // could register client with server?
            // could get anchor differently.

            // throw new NotImplementedException();
        }

        private void ChangeAnchorParameterType(DbParameter param)
        {
            if (param != null)
            {
                if (param.Value == DBNull.Value)
                {
                    param.Value = DefaultAnchorValue;
                }
                else
                {
                    if (param.Value.GetType() != Plugin.SyncColumnInfo.CreationVersionColumnType)
                    {
                        param.Value = System.Convert.ChangeType(param.Value, Plugin.SyncColumnInfo.CreationVersionColumnType);
                    }
                }

            }
        }

        private void ChangeSyncClientParameterType(DbParameter param)
        {
            if (param != null)
            {
                if (param.Value == DBNull.Value)
                {
                    //  param.Value = DefaultAnchorValue;
                }
                else
                {
                    if (param.Value.GetType() != Plugin.SyncColumnInfo.CreatedBySyncClientIdColumnType)
                    {
                        param.Value = param.Value.ToString();
                        if (param.DbType != DbType.String)
                        {
                            param.DbType = DbType.String;
                        }
                    }
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