using System.Collections.Generic;
using System.Reflection;
using CrmSync.Dynamics.ComponentRegistration.Entities;

namespace CrmSync.Dynamics.ComponentRegistration
{
    public class PluginAssemblyRegistration
    {

        public PluginAssemblyRegistration()
        {
            this.PluginTypeRegistrations = new List<PluginTypeRegistration>();
        }

        public ComponentRegistration ComponentRegistration { get; set; }

        public PluginAssembly PluginAssembly { get; set; }
        public Assembly Assembly { get; set; }

        public List<PluginTypeRegistration> PluginTypeRegistrations { get; set; }

    }
}