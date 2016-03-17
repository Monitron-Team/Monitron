using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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

        private static readonly string sr_VmName = "maorico";
        private static readonly string sr_ImageIdOrSlug = "fedora-23-x64";
        private static readonly string sr_NewSize = "1gb";
        private static readonly string sr_NewName = "blach";
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
            var NewDroplet = vmManager.CreateVM(reqParam); //q for saggi - is it ok that the convention is different?
            Assert.IsTrue(NewDroplet.Name == reqParam.Name);
        }

        [Test()]
        public void Delete()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName , out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.DeleteVm(VmId).Succeded);
        }

        [Test()]
        public void PowerOff()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.PowerOffVm(VmId).Succeded);
        }

        [Test()]
        public void PowerOn()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.PowerOnVm(VmId).Succeded);
        }

        [Test()]
        public void EnableIpv6()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.EnableIpv6(VmId).Succeded);
        }

        [Test()]
        public void RebootVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.RebootVm(VmId).Succeded);
        }

        [Test()]
        public void DisableBackupsVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.DisableBackups(VmId).Succeded);
        }

        [Test()]
        public void RebuildVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.RebuildVm(VmId, sr_ImageIdOrSlug).Succeded);
        }

        [Test()]
        public void EnablePrivateNetworkingVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.EnablePrivateNetworking(VmId).Succeded);
        }

        [Test()]
        public void ResetPasswordVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.ResetPasswordVm(VmId).Succeded);
        }

        [Test()]
        public void ShutdownVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.ShutdownVm(VmId).Succeded);
        }
        [Test()]
        public void DisableBackups()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.DisableBackups(VmId).Succeded);
        }

        [Test()]  // NOT WORKING!
        public void ReziseVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.ResizeVm(VmId, sr_NewSize).Succeded);
        }

        [Test()]
        public void RenameVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool ParseSucceded;
            int VmId = vmManager.GetVmIdByName(sr_VmName, out ParseSucceded);
            Assert.IsTrue(ParseSucceded && vmManager.RenameVm(VmId, sr_NewName).Succeded);
        }
    }
}
