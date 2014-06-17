using CrmSync.Dynamics.ComponentRegistration.Entities;
using CrmSync.Dynamics.ComponentRegistration.Enums;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics.ComponentRegistration
{
    public class PluginStepRegistration
    {

        public PluginStepRegistration(PluginTypeRegistration pluginTypeRegistration, string sdkMessageName, string primaryEntityName, string secondaryEntityName = "")
        {
            PluginTypeRegistration = pluginTypeRegistration;
            SdkMessageProcessingStep = new SdkMessageProcessingStep();
            SdkMessageName = sdkMessageName;
            PluginTypeRegistration.PluginType.PropertyChanged += PluginType_PropertyChanged;
            SdkMessageProcessingStep.plugintype_sdkmessageprocessingstep = pluginTypeRegistration.PluginType;
            SdkMessageProcessingStep.plugintypeid_sdkmessageprocessingstep = pluginTypeRegistration.PluginType;
            PrimaryEntityName = primaryEntityName;
            SecondaryEntityName = secondaryEntityName;
        }

        public PluginStepRegistration(PluginTypeRegistration pluginTypeRegistration, SdkMessageNames sdkMessageName, string primaryEntityName, string secondaryEntityName = "")
            : this(pluginTypeRegistration, sdkMessageName.ToString(), primaryEntityName, secondaryEntityName)
        {
        }

        void PluginType_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pluginType = PluginTypeRegistration.PluginType;
            // PluginTypeRegistration.PluginType.PluginTypeId
            if (e.PropertyName == "PluginTypeId")
            {
                if (pluginType.PluginTypeId == null)
                {
                    SdkMessageProcessingStep.EventHandler = null;
                }
                else
                {
                    SdkMessageProcessingStep.EventHandler = new EntityReference(pluginType.LogicalName, pluginType.PluginTypeId.Value);
                }
            }
            //throw new System.NotImplementedException();
        }

        public PluginTypeRegistration PluginTypeRegistration { get; set; }

        public SdkMessageProcessingStep SdkMessageProcessingStep { get; set; }

        public string SdkMessageName { get; set; }

        public string PrimaryEntityName { get; set; }

        public string SecondaryEntityName { get; set; }


    }
}