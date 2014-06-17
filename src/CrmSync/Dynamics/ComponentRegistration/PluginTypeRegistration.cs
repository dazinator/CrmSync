using System;
using System.Collections.Generic;
using CrmSync.Dynamics.ComponentRegistration.Entities;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics.ComponentRegistration
{
    public class PluginTypeRegistration
    {

        public PluginAssemblyRegistration PluginAssemblyRegistration { get; set; }


        public PluginTypeRegistration(PluginAssemblyRegistration pluginAssemblyRegistration, Type type)
        {
            PluginAssemblyRegistration = pluginAssemblyRegistration;
            PluginType = new PluginType { TypeName = type.FullName, FriendlyName = type.FullName };
            Type = type;
            PluginStepRegistrations = new List<PluginStepRegistration>();
            pluginAssemblyRegistration.PluginAssembly.PropertyChanged += PluginAssembly_PropertyChanged;
        }

        void PluginAssembly_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pluginAssembly = PluginAssemblyRegistration.PluginAssembly;

            if (e.PropertyName == "PluginAssemblyId")
            {
                if (pluginAssembly.PluginAssemblyId == null)
                {
                    PluginType.PluginAssemblyId = null;
                }
                else
                {
                    PluginType.PluginAssemblyId = new EntityReference(pluginAssembly.LogicalName, pluginAssembly.PluginAssemblyId.Value);
                }
            }
        }

        public PluginType PluginType { get; set; }

        public Type Type { get; set; }

        public List<PluginStepRegistration> PluginStepRegistrations { get; set; }
    }
}