using System;
using System.Collections.Generic;

namespace CrmSync.Dynamics.ComponentRegistration
{

    public class RegistrationInfo
    {
        private IRegistrationDeployer _Deployer;

        internal RegistrationInfo(IRegistrationDeployer deployer)
        {
            RelatedEntities = new Dictionary<string, Guid>();
            _Deployer = deployer;
        }

        public bool Success { get; set; }
        public Dictionary<string, Guid> RelatedEntities { get; set; }
        public Exception Error { get; set; }

        public void Undeploy()
        {
            _Deployer.Undeploy(this);
        }

    }
}