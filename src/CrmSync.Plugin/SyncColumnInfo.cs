using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrmSync.Plugin
{
    public static class SyncColumnInfo
    {

        public const string RowVersionAttributeName = "versionnumber";
        public const string CreatedRowVersionAttributeName = "crmsync_createdversionnumber";
        public const string CreatedBySyncClientIdAttributeName = "crmsync_createdbysyncclientid";
        public static Type CreationVersionColumnType = typeof(decimal);
        public static Type CreatedBySyncClientIdColumnType = typeof(string);



    }
}
