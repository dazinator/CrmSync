namespace CrmSync.Dynamics.ComponentRegistration
{
    public class PluginTypeOptionsBuilder
    {

        protected PluginTypeRegistration PluginTypeRegistration { get; set; }

        public PluginAssemblyOptionsBuilder PluginAssemblyOptions { get; set; }

        public PluginTypeOptionsBuilder(PluginAssemblyOptionsBuilder pluginAssemblyOptionsBuilder, PluginTypeRegistration pluginTypeRegistration)
        {
            PluginAssemblyOptions = pluginAssemblyOptionsBuilder;
            PluginTypeRegistration = pluginTypeRegistration;
        }

        public PluginStepOptionsBuilder ExecutedOn(string sdkMessageName, string primaryEntityLogicalName, string secondaryEntityLogicalName = "")
        {
            var pluginStepRegistration = new PluginStepRegistration(this.PluginTypeRegistration, sdkMessageName, primaryEntityLogicalName, secondaryEntityLogicalName);
            PluginTypeRegistration.PluginStepRegistrations.Add(pluginStepRegistration);
            return new PluginStepOptionsBuilder(this, pluginStepRegistration);
        }

        public PluginStepOptionsBuilder ExecutedOn(SdkMessageNames sdkMessageName, string primaryEntityLogicalName, string secondaryEntityLogicalName = "")
        {
            var pluginStepRegistration = new PluginStepRegistration(this.PluginTypeRegistration, sdkMessageName, primaryEntityLogicalName, secondaryEntityLogicalName);
            PluginTypeRegistration.PluginStepRegistrations.Add(pluginStepRegistration);
            return new PluginStepOptionsBuilder(this, pluginStepRegistration);
        }

    }
}