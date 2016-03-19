using System.Collections.Generic;

namespace Monitron.VmManagment
{
    public class VmCreationParams
    {
        public bool BackupsEnabled { get; set; }
        public object ImageIdOrSlug { get; set; }
        public bool Ipv6Enabled { get; set; }
        public string Name { get; set; }
        public bool PrivateNetworkingEnabled { get; set; }
        public string RegionSlug { get; set; }
        public string SizeSlug { get; set; }
    }
}
