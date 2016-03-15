using System.Collections.Generic;
using Monitron.VmManagment;
using DigitalOcean.API.Models.Responses;

namespace Monitron.DigitalOceanVmManagment
{
    class DropletWrapper : IVirtualMachine
    {
        private Droplet m_VM;

        public string Name => this.m_VM.Name;
        public int Id => this.m_VM.Id;
        public string Status => this.m_VM.Status;
        public bool Locked => this.m_VM.Locked;
        public int Memory => this.m_VM.Memory;
        public int KernelId => this.m_VM.Kernel.Id;
        public string KernelName => this.m_VM.Kernel.Name;
        public string KernelVersion => this.m_VM.Kernel.Version;

        public bool Ipv6Enabled => this.m_VM.Features.Contains("ipv6");
        public bool PrivateNetworkingEnabled => this.m_VM.Features.Contains("private_networking");
        public IList<Ip> IpV4
        {
            get
            {
                return GetAllIps(m_VM.Networks.v4);
            }
        }

        public IList<Ip> IpV6
        {
            get
            {
                return GetAllIps(m_VM.Networks.v6);
            }
        }

        public DropletWrapper(Droplet i_Vm)
        {
            m_VM = i_Vm;
        }

        private List<Ip> GetAllIps(List<Interface> i_Ips)
        {
            var result = new List<Ip>();
            foreach (Interface inter in i_Ips)
            {
                Ip ip = new Ip();
                ip.IpAddress = inter.IpAddress;
                ip.Gateway = inter.Gateway;
                ip.Netmask = inter.Netmask;
                ip.IsPrivate = (inter.Type == "private");
                result.Add(ip);
            }

            return result;
        }

    }
}
