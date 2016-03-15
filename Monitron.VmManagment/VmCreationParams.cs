using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public List<object> SshIdsOrFingerprints { get; set; }

        public VmCreationParams()
        {
            SshIdsOrFingerprints = new List<object>();
        }
    }
}
