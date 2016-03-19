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
        private static string s_newName = "booom";
        private static IVirtualMachine s_vm = null;
        private static IVmManager s_vmManager = new DigitalOceanVmManager(sr_Token);


        [Test()]
        public void CreateVm()
        {
            DigitalOceanVmManager vmManager = new DigitalOceanVmManager(sr_Token);
            var reqParam = new VmCreationParams();
            reqParam.Ipv6Enabled = false;
            reqParam.Name = s_vmName;
            reqParam.BackupsEnabled = true;
            reqParam.RegionSlug = "nyc2";
            reqParam.ImageIdOrSlug = sr_ImageIdOrSlug;
            reqParam.SizeSlug = "512mb";
            reqParam.PrivateNetworkingEnabled = false;
            s_vm = vmManager.CreateVm(reqParam); 
            Assert.IsTrue(s_vm.Name == reqParam.Name);
        }

        [Test()]
        public void DeleteVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.DeleteVm(s_vm.Id).Succeded);
        }



        [Test()]
        public void PowerOff()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.PowerOffVm(s_vm.Id).Succeded);
        }

        [Test()]
        public void PowerOn()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.PowerOnVm(s_vm.Id).Succeded);
        }

        [Test()]
        public void EnableIpv6()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.EnableIpv6(s_vm.Id).Succeded);
        }

        [Test()]
        public void RebootVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.RebootVm(s_vm.Id).Succeded);
        }

        [Test()]
        public void DisableBackupsVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.DisableBackups(s_vm.Id).Succeded);
        }

        [Test()]
        public void RebuildVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.RebuildVm(s_vm.Id, sr_ImageIdOrSlug).Succeded);
        }

        [Test()]
        public void EnablePrivateNetworkingVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.EnablePrivateNetworking(s_vm.Id).Succeded);
        }

        [Test()]
        public void ResetPasswordVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.ResetPasswordVm(s_vm.Id).Succeded);
        }

        [Test()]
        public void ShutdownVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.ShutdownVm(s_vm.Id).Succeded);
        }
        [Test()]
        public void DisableBackups()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.DisableBackups(s_vm.Id).Succeded);
        }

        [Test()]  // NOT WORKING!
        public void ReziseVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.ResizeVm(s_vm.Id, sr_NewSize).Succeded);
        }

        [Test()]
        public void RenameVm()
        {
            initDemoVm();
            Assert.IsTrue(s_vmManager.RenameVm(s_vm.Id, s_newName).Succeded);
            s_vmName = s_newName;
            s_newName = s_newName + "0";
        }

        private void initDemoVm()
        {
            if (s_vm == null)
            {
                bool vmExist;
                int id = s_vmManager.GetVmIdByName(s_vmName, out vmExist);
                if (!vmExist)
                {
                    CreateVm();
                    Thread.Sleep(60000);  //Saggi! I didnt find a way to see if the procces ended before sending other commands)
                }
                else s_vm = s_vmManager.GetVmById(id);
            }
        }

        [Test()]
        public void Sanity()
        {
            int SleepTime = 90000;
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
            catch (Exception)
            {
                Console.WriteLine("PowerOn(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.RenameVm();
                Console.WriteLine("RenameVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("RenameVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.RebuildVm();
                Console.WriteLine("RebuildVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("RebuildVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.DisableBackups();
                Console.WriteLine("DisableBackups(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("DisableBackups(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.ResetPasswordVm();
                Console.WriteLine("ResetPasswordVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("ResetPasswordVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.EnablePrivateNetworkingVm();
                Console.WriteLine("EnablePrivateNetworkingVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("EnablePrivateNetworkingVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.RebootVm();
                Console.WriteLine("RebootVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("RebootVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.ShutdownVm();
                Console.WriteLine("ShutdownVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("ShutdownVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);

            try
            {
                this.DeleteVm();
                Console.WriteLine("DeleteVm(): SUCCESS!");
            }
            catch (Exception)
            {
                Console.WriteLine("DeleteVm(): FAILIURE!");
            }
            Thread.Sleep(SleepTime);
        }
    }
}
