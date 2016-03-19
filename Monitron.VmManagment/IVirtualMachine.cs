using System.Collections.Generic;

namespace Monitron.VmManagment
{
    public interface IVirtualMachine
    {
        string Name { get; }
        int Id { get; }
        string Status { get; }
        bool Locked { get; }
        int Memory { get; }
        int KernelId { get; }
        string KernelName { get; }
        string KernelVersion { get; }
        IList<Ip> IpV4 { get; }
        IList<Ip> IpV6 { get; }
        bool Ipv6Enabled { get; }
        bool PrivateNetworkingEnabled { get; }

    }
}
