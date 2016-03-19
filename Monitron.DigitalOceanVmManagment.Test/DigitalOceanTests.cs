using System;
using System.Threading;
using Monitron.VmManagment;
using NUnit.Framework;

namespace Monitron.DigitalOceanVmManagment.Test
{

    class DigitalOceanTests
    {
        static readonly string sr_Token = "da8beb13c138f71fee27747f75150c4e848b0e3d1d98a0af86b463219c4eaeb4";

        private static string s_vmName = "nomnomnom";
        private static readonly string sr_ImageIdOrSlug = "fedora-23-x64";
        private static readonly string sr_NewSize = "1gb";
        private static string NewName = "booom";
        [Test()]
        public void CreateVm()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            var reqParam = new VmCreationParams();
            reqParam.Ipv6 = false;
            reqParam.Name = s_vmName;
            reqParam.Backups = true;
            reqParam.RegionSlug = "nyc2";
            reqParam.ImageIdOrSlug = sr_ImageIdOrSlug;
            reqParam.SizeSlug = "512mb";
            reqParam.PrivateNetworking = false;
            var newDroplet = vmManager.CreateVm(reqParam); //q for saggi - is it ok that the convention is different?
            Assert.IsTrue(newDroplet.Name == reqParam.Name);
        }

        [Test()]
        public void Delete()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.DeleteVm(vmId).Succeded);
        }

        [Test()]
        public void PowerOff()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.PowerOffVm(vmId).Succeded);
        }

        [Test()]
        public void PowerOn()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.PowerOnVm(vmId).Succeded);
        }

        [Test()]
        public void EnableIpv6()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.EnableIpv6(vmId).Succeded);
        }

        [Test()]
        public void RebootVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.RebootVm(vmId).Succeded);
        }

        [Test()]
        public void DisableBackupsVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.DisableBackups(vmId).Succeded);
        }

        [Test()]
        public void RebuildVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.RebuildVm(vmId, sr_ImageIdOrSlug).Succeded);
        }

        [Test()]
        public void EnablePrivateNetworkingVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.EnablePrivateNetworking(vmId).Succeded);
        }

        [Test()]
        public void ResetPasswordVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.ResetPasswordVm(vmId).Succeded);
        }

        [Test()]
        public void ShutdownVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.ShutdownVm(vmId).Succeded);
        }
        [Test()]
        public void DisableBackups()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.DisableBackups(vmId).Succeded);
        }

        [Test()]  // NOT WORKING!
        public void ReziseVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.ResizeVm(vmId, sr_NewSize).Succeded);
        }

        [Test()]
        public void RenameVm()
        {
            IVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            bool parseSucceded;
            int vmId = vmManager.GetVmIdByName(s_vmName, out parseSucceded);
            Assert.IsTrue(parseSucceded && vmManager.RenameVm(vmId, NewName).Succeded);
            s_vmName = NewName;
        }

        [Test()]
        public void Sanity()
        {
            int SleepTime = 60000;
            this.CreateVm();
            Thread.Sleep(SleepTime);
            try
            {
                this.EnableIpv6();
                Console.WriteLine("EnableIpv6(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("EnableIpv6(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.PowerOff();
                Console.WriteLine("PowerOff(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("PowerOff(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.PowerOn();
                Console.WriteLine("PowerOn(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("PowerOn(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.RenameVm();
                Console.WriteLine("RenameVm(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("RenameVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.RebuildVm();
                Console.WriteLine("RebuildVm(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("RebuildVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.DisableBackups();
                Console.WriteLine("DisableBackups(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("DisableBackups(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.ResetPasswordVm();
                Console.WriteLine("ResetPasswordVm(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ResetPasswordVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.EnablePrivateNetworkingVm();
                Console.WriteLine("EnablePrivateNetworkingVm(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("EnablePrivateNetworkingVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.RebootVm();
                Console.WriteLine("RebootVm(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("RebootVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.ShutdownVm();
                Console.WriteLine("ShutdownVm(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ShutdownVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.Delete();
                Console.WriteLine("Delete(): SUCCESS!");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);
        }
    }
}
