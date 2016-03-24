using System.Collections.Generic;
using System.Net;

namespace Monitron.CloudManagers
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
        IList<IPAddress> IpV4 { get; }
        IList<IPAddress> IpV6 { get; }
        bool Ipv6Enabled { get; }
    }
}
