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

        private static readonly string sr_VmName = "blach";
        private static readonly string sr_ImageIdOrSlug = "fedora-23-x64";
        [Test()]
        public void CreateVm()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            var reqParam = new VmCreationParams();
            reqParam.Ipv6 = false;
            string Time = DateTime.Now.ToShortTimeString().Replace(':', '_').Replace(' ', '_').Replace('/', '_');
            reqParam.Name = sr_VmName;
            reqParam.Backups = false;
            reqParam.RegionSlug = "nyc2";
            reqParam.ImageIdOrSlug = sr_ImageIdOrSlug;
            reqParam.SizeSlug = "512mb";
            reqParam.PrivateNetworking = true;   
            var NewDroplet = vmManager.CreateVM(reqParam);
            /*
            Thread.Sleep(20000);
            bool res_StopVm = vmManager.StopVm(NewDroplet.Id);
            Thread.Sleep(60000);
            bool res_StartVm = vmManager.StartVm(NewDroplet.Id);
            Thread.Sleep(60000);
            bool res_EnablePrivateNetworking = vmManager.EnablePrivateNetworking(NewDroplet.Id);
            Thread.Sleep(60000);
            bool res_DeleteVm = vmManager.DeleteVm(NewDroplet.Id);
            */

        }

        [Test()]
        public void Delete()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            vmManager.DeleteVm(vmManager.GetVmIdByName(sr_VmName));
        }

        [Test()]
        public void PowerOff()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            Assert.IsTrue(vmManager.ShutdownVm(vmManager.GetVmIdByName(sr_VmName)));
        }

        [Test()]
        public void PowerOn()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            Assert.IsTrue(vmManager.StartVm(vmManager.GetVmIdByName(sr_VmName)));
        }

        [Test()]
        public void EnableIpv6()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            Assert.IsTrue(vmManager.EnableIpv6(vmManager.GetVmIdByName(sr_VmName)));
        }

        [Test()]
        public void RebootVm()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            Assert.IsTrue(vmManager.RebootVm(vmManager.GetVmIdByName(sr_VmName)));
        }
        [Test()]
        public void RebuildVm()
        {
            bool successGettingId;
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            Assert.IsTrue(vmManager.RebuildVm(vmManager.GetVmIdByName(sr_VmName),sr_ImageIdOrSlug));
        }
    }
}
