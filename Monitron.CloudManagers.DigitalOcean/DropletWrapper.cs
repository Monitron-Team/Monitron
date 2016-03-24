using System.Collections.Generic;
using Monitron.CloudManagers;
using DigitalOcean.API.Models.Responses;
using System.Net;
using System.Net.NetworkInformation;

namespace Monitron.CloudManagers.DigitalOcean
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
        public IList<IPAddress> IpV4
        {
            get
            {
                return GetAllIps(m_VM.Networks.v4);
            }
        }

        public IList<IPAddress> IpV6
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

        private List<IPAddress> GetAllIps(List<Interface> i_Ips)
        {
            var result = new List<IPAddress>();
            foreach (Interface inter in i_Ips)
            {
                IPAddress ip = IPAddress.Parse(inter.IpAddress);
                result.Add(ip);
            }

            return result;
        }

    }
}
