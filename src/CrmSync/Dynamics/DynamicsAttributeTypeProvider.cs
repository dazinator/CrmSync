using System;
using System.Globalization;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics
{
    public class DynamicsAttributeTypeProvider : IDynamicsAttributeTypeProvider
    {
        //public const string DateTimeFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffK";

        public long GetBigInt(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.Int64)
            {
                return (long)value;
            }
            if (typeCode == TypeCode.Int32)
            {
                return Convert.ToInt64((int)value);
            }
            if (typeCode == TypeCode.String)
            {
                long sdkVal;
                if (long.TryParse((string)value, out sdkVal))
                {
                    return sdkVal;
                }
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "BigInt");
        }

        public bool GetBoolean(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.Boolean)
            {
                return (bool)value;
            }
            if (typeCode == TypeCode.Int32)
            {
                // bool sdkVal = false;
                var intVal = (int)value;
                if (intVal == 1)
                {
                    return true;
                }
                if (intVal == 0)
                {
                    return false;
                }
            }
            if (typeCode == TypeCode.String)
            {
                bool sdkVal;
                if (bool.TryParse((string)value, out sdkVal))
                {
                    return sdkVal;
                }
            }

            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "Boolean");
        }

        public DateTime GetDateTime(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.DateTime)
            {
                return (DateTime)value;
            }
            if (typeCode == TypeCode.String)
            {
                DateTime sdkVal;

                if (DateTime.TryParse((string)value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out sdkVal))
                {
                    return sdkVal;
                }

            }

            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "DateTime");
        }

        public decimal GetDecimal(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.Decimal)
            {
                return (decimal)value;
            }
            if (typeCode == TypeCode.Int32)
            {
                return System.Convert.ToDecimal(value);
            }
            if (typeCode == TypeCode.String)
            {
                decimal sdkVal;
                if (decimal.TryParse((string)value, out sdkVal))
                {
                    return sdkVal;
                }
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "Decimal");
        }

        public double GetDouble(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.Double)
            {
                return (double)value;
            }
            if (typeCode == TypeCode.Decimal || typeCode == TypeCode.Int32)
            {
                return Convert.ToDouble(value);
            }
            if (typeCode == TypeCode.String)
            {
                double sdkVal;
                if (double.TryParse((string)value, out sdkVal))
                {
                    return sdkVal;
                }
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "Double");
        }

        public string GetEntityName(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.String)
            {
                return (string)value;
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "EntityName");
        }

        public int GetInteger(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.Int32)
            {
                return (int)value;
            }
            if (typeCode == TypeCode.String)
            {
                int sdkVal;
                if (int.TryParse((string)value, out sdkVal))
                {
                    return sdkVal;
                }
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "Integer");
        }

        public string GetMemo(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.String)
            {
                return (string)value;
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "Memo");
        }

        public string GetString(object value)
        {
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.String)
            {
                return (string)value;
            }
            return value.ToString();
        }

        public Money GetMoney(object value)
        {
            var decimalVal = GetDecimal(value);
            return new Money(decimalVal);
        }

        public OptionSetValue GetPicklist(object value)
        {
            var intVal = GetInteger(value);
            return new OptionSetValue(intVal);
        }

        public OptionSetValue GetState(object value)
        {
            var intVal = GetInteger(value);
            return new OptionSetValue(intVal);
        }

        public OptionSetValue GetStatus(object value)
        {
            var intVal = GetInteger(value);
            return new OptionSetValue(intVal);
        }

        public Guid GetUniqueIdentifier(object value)
        {
            if (value is Guid)
            {
                return (Guid)value;
            }
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.String)
            {
                Guid sdkVal;
                if (Guid.TryParse((string)value, out sdkVal))
                {
                    return sdkVal;
                }
            }
            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "UniqueIdentifier");
        }

        public BooleanManagedProperty GetManagedProperty(object value)
        {
            throw new NotImplementedException();
        }

        public EntityReference GetLookup(object value)
        {
            if (value is Guid)
            {
                return new EntityReference(string.Empty, (Guid)value);
            }
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.String)
            {
                Guid sdkVal;
                if (Guid.TryParse((string)value, out sdkVal))
                {
                    return new EntityReference(string.Empty, sdkVal);
                }
            }

            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "Integer");
        }

        public EntityReference GetCustomer(object value)
        {
            if (value is Guid)
            {
                return new EntityReference(string.Empty, (Guid)value);
            }
            var typeCode = Type.GetTypeCode(value.GetType());
            if (typeCode == TypeCode.String)
            {
                Guid sdkVal;
                var stringVal = (string)value;
                if (Guid.TryParse(stringVal, out sdkVal))
                {
                    return new EntityReference(string.Empty, sdkVal);
                }
                switch (stringVal.ToLowerInvariant())
                {
                    case "contact":
                    case "account":
                        return new EntityReference(stringVal, Guid.Empty);
                }
            }

            throw new InvalidOperationException("unable to convert value of type: " + typeCode + " to dynamics " + "EntityReference");
        }
        
        public EntityReference GetOwner(object value)
        {
            return GetLookup(value);
        }

        #region Not Supported

        public EntityCollection GetCalendarRules(object value)
        {
            throw new NotImplementedException();
        }

        public EntityCollection GetPartyList(object value)
        {
            throw new NotImplementedException();
        }

        public object GetVirtual(object value)
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}