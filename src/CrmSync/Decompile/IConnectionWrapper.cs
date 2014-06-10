using System;
using System.Data;

namespace CrmSync
{
    internal interface IConnectionWrapper : IDbConnection, IDisposable
    {
        IDbConnection WrappedConnection { get; }

        ICommandWrapperFactory MakeCmdWrapperFactory();
    }
}