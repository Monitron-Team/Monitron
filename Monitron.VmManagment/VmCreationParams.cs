using System.Collections.Generic;

namespace Monitron.VmManagment
{
    public class VmCreationParams
    {
        public bool Backups { get; set; }
        public object ImageIdOrSlug { get; set; }
        public bool Ipv6 { get; set; }
        public string Name { get; set; }
        public bool PrivateNetworking { get; set; }
        public string RegionSlug { get; set; }
        public string SizeSlug { get; set; }
    }
}
