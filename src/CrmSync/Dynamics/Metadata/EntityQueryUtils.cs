using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace CrmSync.Dynamics.Metadata
{
    public static class EntityQueryUtils
    {
        public static LinkEntity FindLinkEntity(this LinkEntity linkEntity, string aliasOrLogicalName, bool isAlias)
        {
            if (isAlias)
            {
                if (!String.IsNullOrEmpty(linkEntity.EntityAlias) && aliasOrLogicalName == linkEntity.EntityAlias)
                {
                    return linkEntity;
                }
            }
            else
            {
                if (aliasOrLogicalName == linkEntity.LinkToEntityName)
                {
                    return linkEntity;
                }
            }

            if (linkEntity.LinkEntities != null && linkEntity.LinkEntities.Any())
            {
                if (isAlias)
                {
                    foreach (var l in linkEntity.LinkEntities)
                    {
                        if (!String.IsNullOrEmpty(l.EntityAlias) && aliasOrLogicalName == l.EntityAlias)
                        {
                            return l;
                        }
                        var found = FindLinkEntity(l, aliasOrLogicalName, isAlias);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
                else
                {
                    foreach (var l in linkEntity.LinkEntities)
                    {
                        if (aliasOrLogicalName == l.LinkToEntityName)
                        {
                            return l;
                        }
                        var found = FindLinkEntity(l, aliasOrLogicalName, isAlias);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linkEntity"></param>
        /// <param name="recursive">If true, will also include all columns of any Link Entities linked to this entity.</param>
        public static void IncludeAllColumns(this LinkEntity linkEntity, bool recursive = false)
        {
            linkEntity.Columns.Columns.Clear();
            linkEntity.Columns.AllColumns = true;
            if (recursive)
            {
                if (linkEntity.LinkEntities != null && linkEntity.LinkEntities.Any())
                {
                    foreach (var l in linkEntity.LinkEntities)
                    {
                        l.Columns.Columns.Clear();
                        l.Columns.AllColumns = true;
                        IncludeAllColumns(linkEntity, true);
                    }
                }
            }
        }

        public static LinkEntity FindLinkEntity(this QueryExpression query, string aliasOrLogicalName, bool isAlias)
        {
            foreach (var l in query.LinkEntities)
            {
                var link = FindLinkEntity(l, aliasOrLogicalName, isAlias);
                if (link != null)
                {
                    return link;
                }
            }
            return null;
        }

        /// <summary>
        /// Includes all columns for this and optionally all link entities recursviely.
        /// </summary>
        /// <param name="query"></param>
        public static void IncludeAllColumns(this QueryExpression query)
        {
            query.ColumnSet.Columns.Clear();
            query.ColumnSet.AllColumns = true;
            foreach (var l in query.LinkEntities)
            {
                IncludeAllColumns(l, true);
            }
        }



    }
}