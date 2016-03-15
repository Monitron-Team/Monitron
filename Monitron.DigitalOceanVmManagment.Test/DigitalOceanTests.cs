using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Monitron.VmManagment;

using NUnit.Framework;

namespace Monitron.DigitalOceanVmManagment.Test
{
   
    class DigitalOceanTests
    {
        static readonly string sr_Token = "da8beb13c138f71fee27747f75150c4e848b0e3d1d98a0af86b463219c4eaeb4";
        [Test()]
        public void CreateVmNDelete()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            var reqParam = new VmCreationParams();
            reqParam.Ipv6 = true;
            string Time = DateTime.Now.ToString().Replace(':', '_').Replace(' ', '_').Replace('/', '_');
            reqParam.Name = "New-VM-2";
            reqParam.Backups = false;
            reqParam.RegionSlug = "nyc2";
            reqParam.ImageIdOrSlug = "fedora-23-x64";
            reqParam.SizeSlug = "512mb";
            reqParam.PrivateNetworking = true;   
            var NewDroplet = vmManager.CreateVM(reqParam);
            Thread.Sleep(60000);
            // vmManager.DeleteVM(ff.Id);

        }
    }
}
