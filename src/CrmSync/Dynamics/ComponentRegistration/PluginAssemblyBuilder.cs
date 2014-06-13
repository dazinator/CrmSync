using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CrmSync.Dynamics.ComponentRegistration.Enums;
using CrmSync.Dynamics.Entities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmSync.Dynamics.Metadata
{
    /// <summary>
    /// Single responsbility: To provide a fluent API for constructing Crm Plugin Registrations.
    /// </summary>
    public class PluginAssemblyBuilder
    {

        public PluginAssembly PluginAssembly { get; set; }

        //public EntityAttributeMetadataBuilder AttributeBuilder { get; set; }

        protected PluginAssemblyBuilder(PluginAssembly pluginAssembly)
        {
            PluginAssembly = pluginAssembly;
            //  AttributeBuilder = new EntityAttributeMetadataBuilder(this);

        }

        public PluginAssembly Build()
        {
            return PluginAssembly;
        }

        public static PluginAssemblyBuilder CreatePlugin(string pluginName, string publicKeyToken, string version)
        {
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
            var builder = new PluginAssemblyBuilder(pluginAssembly);
            return builder;
        }

        public static PluginAssemblyBuilder CreatePlugin<T>() where T : IPlugin
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
            var builder = new PluginAssemblyBuilder(pluginAssembly);
            builder.PlaceAssemblyInDatabase(assembly);
            return builder;
        }

        public PluginAssemblyBuilder PlaceAssemblyInDatabase(Assembly assembly)
        {
            PluginAssembly.SourceType = new OptionSetValue()
            {
                Value = (int)AssemblySourceType.Database
            };
            PluginAssembly.Content = Convert.ToBase64String(File.ReadAllBytes(assembly.Location));
            return this;
        }

        public PluginAssemblyBuilder PlaceAssemblyInDatabase(string assemblyFilePath)
        {
            PluginAssembly.SourceType = new OptionSetValue()
                {
                    Value = (int)AssemblySourceType.Database
                };
            PluginAssembly.Content = Convert.ToBase64String(File.ReadAllBytes(assemblyFilePath));
            return this;
        }

        public PluginAssemblyBuilder IsLocatedOnServerFileSystem(string serverFilename)
        {
            PluginAssembly.SourceType = new OptionSetValue()
            {
                Value = (int)AssemblySourceType.Disk
            };
            PluginAssembly.Path = serverFilename;
            return this;
        }

        public PluginAssemblyBuilder IsLocatedOnServerGac()
        {
            PluginAssembly.SourceType = new OptionSetValue()
            {
                Value = (int)AssemblySourceType.GAC
            };
            return this;
        }

        public PluginAssemblyBuilder HasDescription(string description)
        {
            PluginAssembly.Description = description;
            return this;
        }

        public PluginAssemblyBuilder HasVersion(string version)
        {
            PluginAssembly.Version = version;
            return this;
        }

        public PluginAssemblyBuilder HasPublicKeyToken(string publicKeyToken)
        {
            PluginAssembly.PublicKeyToken = publicKeyToken;
            return this;
        }

        public PluginAssemblyBuilder RunsInIsolationMode(IsolationMode isolationMode)
        {
            PluginAssembly.IsolationMode = new OptionSetValue()
                {
                    Value = (int)isolationMode
                };
            return this;
        }

    }
}