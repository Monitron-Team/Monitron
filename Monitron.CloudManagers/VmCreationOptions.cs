using System.Collections.Generic;

namespace Monitron.CloudManagers
{
    public class VmCreationOptions
    {
        public object ImageIdOrSlug { get; set; }
        public bool Ipv6Enabled { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string SizeSlug { get; set; }
    }
}
