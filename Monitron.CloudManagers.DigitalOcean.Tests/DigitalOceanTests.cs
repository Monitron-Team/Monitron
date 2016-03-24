using System;
using System.Threading;
using Monitron.CloudManagers;
using NUnit.Framework;
using Monitron.CloudManagers.DigitalOcean;
using System.Configuration;

namespace Monitron.DigitalOceanVmManagment.Test
{

    class DigitalOceanTests
    {
        private static string s_vmName = "nomnomnom";
        private ICloudManager m_CloudManager;

        [SetUp()]
        public void SetUp()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string token = appSettings["digitalocean_api_token"];

            m_CloudManager = new DigitalOceanCloudManager(token);
        }


        public IVirtualMachine CreateVm()
        {
            var reqParam = new VmCreationOptions();
            reqParam.Ipv6Enabled = false;
            reqParam.Name = s_vmName;
            reqParam.Region = "nyc2";
            reqParam.ImageIdOrSlug = "fedora-23-x64";
            reqParam.SizeSlug = "512mb";
            IVirtualMachine vm = m_CloudManager.CreateVm(reqParam); 
            Assert.IsTrue(vm.Name == reqParam.Name);
            return vm;
        }

        [Test()]
        public void DeleteVm()
        {
            IVirtualMachine vm = initDemoVm();
            m_CloudManager.DeleteVm(vm.Id);
        }



        [Test()]
        public void RebootVm()
        {
            IVirtualMachine vm = initDemoVm();
            m_CloudManager.RebootVm(vm.Id);
            m_CloudManager.DeleteVm(vm.Id);
        }

        [Test()]
        public void PowerCycle()
        {
            IVirtualMachine vm = initDemoVm();
            m_CloudManager.PowerOffVm(vm.Id);
            m_CloudManager.DeleteVm(vm.Id);
        }

        [Test()]
        public void GetVmThatDoesNotExistTest()
        {
            Assert.Throws<AggregateException>(delegate() {
                m_CloudManager.GetVm(123);  
            });
        }

        private IVirtualMachine initDemoVm()
        {
            IVirtualMachine vm = CreateVm();
            bool vmDown = true;
            while (vmDown)
            {
                try
                {
                    vmDown = m_CloudManager.GetVm(vm.Id).Status != "active";
                }
                catch
                {
                    // Ignore
                }
            }

            return vm;
        }
    }
}
