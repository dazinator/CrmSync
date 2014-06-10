using System;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace CrmSync
{
    internal class SyncResource
    {
        private static ResourceManager _resourceManager = new ResourceManager("Microsoft.Synchronization.Data.SyncErrorMessage", Assembly.GetAssembly(typeof(Microsoft.Synchronization.Data.AnchorException)));

        static SyncResource()
        {
        }

        private SyncResource()
        {
        }

        internal static string GetString(string key)
        {
            string @string = SyncResource._resourceManager.GetString(key);
            if (@string == null)
                throw new ArgumentNullException("key", "Error: Resource string for '" + key + "' is null");
            else
                return @string;
        }

        internal static string FormatString(string key, params object[] a1)
        {
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, SyncResource.GetString(key), a1);
        }
    }
}