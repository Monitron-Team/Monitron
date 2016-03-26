using System;

namespace Monitron.Plugins.Management
{
    public class PluginManifest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public string DllName { get; set; }
    }
}

