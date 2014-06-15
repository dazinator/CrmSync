using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CrmSync.Dynamics.ComponentRegistration.Enums;
using CrmSync.Dynamics.Entities;
using Microsoft.Xrm.Sdk;

namespace CrmSync.Dynamics.ComponentRegistration
{
    /// <summary>
    /// Single responsbility: To provide a fluent API for constructing Crm Plugin Registrations.
    /// </summary>
    public class ComponentRegistrationBuilder
    {

        protected ComponentRegistration ComponentRegistration { get; set; }

        protected ComponentRegistrationBuilder()
        {
            //  PluginAssembly = pluginAssembly;
            //  AttributeBuilder = new EntityAttributeMetadataBuilder(this);
            ComponentRegistration = new ComponentRegistration();
        }

        public ComponentRegistration Build()
        {
            return ComponentRegistration;
        }

        public static ComponentRegistrationBuilder CreateRegistration()
        {
            return new ComponentRegistrationBuilder();
        }

        public PluginAssemblyOptionsBuilder WithPluginAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            var pluginName = assemblyName.Name;
            string version = assemblyName.Version.ToString();

            string publicKeyToken;
            byte[] publicKeyTokenBytes = assemblyName.GetPublicKeyToken();
            if (null == publicKeyTokenBytes || 0 == publicKeyTokenBytes.Length)
            {
                publicKeyToken = null;
            }
            else
            {
                publicKeyToken = string.Join(string.Empty, publicKeyTokenBytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
            }


            var pluginAssembly = new PluginAssembly()
            {
                PluginAssemblyId = Guid.NewGuid(),
                Name = pluginName,
                IsolationMode = new OptionSetValue()
                {
                    Value = (int)IsolationMode.None
                },
                Culture = "neutral",
                PublicKeyToken = publicKeyToken,
                Version = version
            };
            //PluginAssembly = PluginAssembly;
           // var builder = new ComponentRegistrationBuilder();
            var par = new PluginAssemblyRegistration()
                {
                    Assembly = assembly,
                    PluginAssembly = pluginAssembly,
                    ComponentRegistration = this.ComponentRegistration
                };
            this.ComponentRegistration.PluginAssemblyRegistrations.Add(par);
            return new PluginAssemblyOptionsBuilder(this, par);
        }

        public PluginAssemblyOptionsBuilder WithPluginAssemblyThatContainsPlugin<T>() where T : IPlugin
        {

            var assembly = Assembly.GetAssembly(typeof(T));
            var assemblyName = assembly.GetName();
            //  var pluginAssemblyPath = Path.GetFullPath(assembly.Location);
            // var publicKeyToken = assembly.GetName().GetPublicKeyToken();
            var pluginName = assemblyName.Name;
            string version = assemblyName.Version.ToString();

            string publicKeyToken;
            byte[] publicKeyTokenBytes = assemblyName.GetPublicKeyToken();
            if (null == publicKeyTokenBytes || 0 == publicKeyTokenBytes.Length)
            {
                publicKeyToken = null;
            }
            else
            {
                publicKeyToken = string.Join(string.Empty, publicKeyTokenBytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
            }

            var pluginAssembly = new PluginAssembly()
            {
                PluginAssemblyId = Guid.NewGuid(),
                Name = pluginName,
                IsolationMode = new OptionSetValue()
                {
                    Value = (int)IsolationMode.None
                },
                Culture = "neutral",
                PublicKeyToken = publicKeyToken,
                Version = version
            };
            //PluginAssembly = PluginAssembly;
           // var builder = new ComponentRegistrationBuilder();

             var par = new PluginAssemblyRegistration()
                {
                    Assembly = assembly,
                    PluginAssembly = pluginAssembly,
                    ComponentRegistration = this.ComponentRegistration
                };
            this.ComponentRegistration.PluginAssemblyRegistrations.Add(par);
            return new PluginAssemblyOptionsBuilder(this, par);
        }

        //public static PluginRegistrationBuilder CreatePlugin(string pluginName, string publicKeyToken, string version)
        //{
        //    var pluginAssembly = new PluginAssembly()
        //    {
        //        PluginAssemblyId = Guid.NewGuid(),
        //        Name = pluginName,
        //        IsolationMode = new OptionSetValue()
        //        {
        //            Value = (int)IsolationMode.None
        //        },
        //        Culture = "neutral",
        //        PublicKeyToken = publicKeyToken,
        //        Version = version
        //    };
        //    //PluginAssembly = PluginAssembly;
        //    var builder = new PluginRegistrationBuilder(pluginAssembly);
        //    return builder;
        //}

    }
}