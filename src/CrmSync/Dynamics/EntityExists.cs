using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics
{
    public class EntityExists
    {

        protected EntityExists(bool exists)
            : this(exists, null)
        {
        }

        protected EntityExists(bool exists, EntityReference reference)
        {
            Exists = exists;
            EntityReference = reference;
        }

        public EntityReference EntityReference { get; set; }

        public bool Exists { get; set; }
        
        public static EntityExists Yes(EntityReference reference)
        {
            return new EntityExists(true, reference);
        }

        public static EntityExists No()
        {
            return new EntityExists(false);
        }

    }
}