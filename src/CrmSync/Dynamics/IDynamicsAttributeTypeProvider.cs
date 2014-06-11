using System;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics
{
    public interface IDynamicsAttributeTypeProvider
    {
        long GetBigInt(object val);

        bool GetBoolean(object value);

        EntityCollection GetCalendarRules(object value);

        EntityReference GetCustomer(object value);

        DateTime GetDateTime(object value);

        decimal GetDecimal(object value);

        double GetDouble(object value);

        string GetEntityName(object value);

        int GetInteger(object value);

        EntityReference GetLookup(object value);

        BooleanManagedProperty GetManagedProperty(object value);

        string GetMemo(object value);

        Money GetMoney(object value);

        EntityReference GetOwner(object value);

        EntityCollection GetPartyList(object value);

        OptionSetValue GetPicklist(object value);

        OptionSetValue GetState(object value);

        OptionSetValue GetStatus(object value);

        string GetString(object value);

        Guid GetUniqueIdentifier(object value);

        object GetVirtual(object value);
    }
}