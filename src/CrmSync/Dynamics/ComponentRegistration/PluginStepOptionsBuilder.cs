using System;
using CrmSync.Dynamics.ComponentRegistration.Enums;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics.ComponentRegistration
{
    public class PluginStepOptionsBuilder
    {

        protected PluginStepRegistration PluginStepRegistration { get; set; }

        public PluginTypeOptionsBuilder PluginTypeOptions { get; set; }

        public PluginStepOptionsBuilder(PluginTypeOptionsBuilder pluginTypeOptionsBuilder, PluginStepRegistration pluginStepRegistration)
        {
            PluginTypeOptions = pluginTypeOptionsBuilder;
            PluginStepRegistration = pluginStepRegistration;
            Rank(1);
        }

        public PluginStepOptionsBuilder SupportedDeployment(PluginStepDeployment deployment)
        {
            var pl = PluginStepRegistration.SdkMessageProcessingStep;
            pl.SupportedDeployment = new OptionSetValue()
                {
                    Value = (int)deployment
                };
            return this;
        }

        [Obsolete]
        public PluginStepOptionsBuilder InvocationSource(PluginStepInvocationSource invocationSource)
        {
            var pl = PluginStepRegistration.SdkMessageProcessingStep;
            pl.InvocationSource = new OptionSetValue()
                {
                    Value = (int)invocationSource
                };
            return this;
        }

        public PluginStepOptionsBuilder Stage(PluginStepStage stage)
        {
            var pl = PluginStepRegistration.SdkMessageProcessingStep;
            pl.Stage = new OptionSetValue()
                {
                    Value = (int)stage
                };
            return this;
        }

        public PluginStepOptionsBuilder Mode(PluginStepMode mode)
        {
            var pl = PluginStepRegistration.SdkMessageProcessingStep;
            pl.Mode = new OptionSetValue()
                {
                    Value = (int)mode
                };
            return this;
        }

        /// <summary>
        /// The rank helps determine the order in which plugins are executed when there are multiple registrations for the same entity / sdk message. 
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public PluginStepOptionsBuilder Rank(int rank)
        {
            var pl = PluginStepRegistration.SdkMessageProcessingStep;
            pl.Rank = rank;
            


            return this;
        }



    }
}