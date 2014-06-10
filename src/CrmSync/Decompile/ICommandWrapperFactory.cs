using System.Data;

namespace CrmSync
{
    internal interface ICommandWrapperFactory
    {
        IDbCommand MakeCommandWrapper(IDbCommand cmd);
    }
}