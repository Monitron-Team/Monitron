using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monitron.VmManagment;
using DigitalOcean.API;
using DigitalOcean.API.Models.Responses;

namespace Monitron.DigitalOceanVmManagment
{
    class DropletWrapper: IVirtualMachine
    {
        private Droplet m_VM;

        public string Name => this.m_VM.Name;
        public int Id => this.m_VM.Id;
        public string Status => this.m_VM.Status;
        public bool Locked => this.m_VM.Locked;
        public int Memory =>this.m_VM.Memory ;
        public int KernelId => this.m_VM.Kernel.Id ;
        public string KernelName => this.m_VM.Kernel.Name;
        public string KernelVersion => this.m_VM.Kernel.Version;

        public string Ipv4
        {
            get
            {
                string result = null;
                if (m_VM.Networks.v4.First() != null)
                {
                    result = this.m_VM.Networks.v4.First().IpAddress;
                }
                return result;
            }
        }

        public DropletWrapper(Droplet i_VM)
        {
            m_VM = i_VM;
        }
    }
}
