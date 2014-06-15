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

        public PluginStepOptionsBuilder AddStep(string sdkMessageName)
        {

            var pluginStepRegistration = new PluginStepRegistration(this.PluginTypeRegistration, sdkMessageName);
            return new PluginStepOptionsBuilder(this, pluginStepRegistration);


        }

    }
}