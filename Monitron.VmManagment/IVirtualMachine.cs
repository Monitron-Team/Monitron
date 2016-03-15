using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Monitron.VmManagment
{
    public interface IVirtualMachine
    {
        string Name { get;  }

        int Id { get; }
        string Status { get; }
        bool Locked { get; }
        int Memory { get; }
        int KernelId { get; }
        string KernelName { get; }
        string KernelVersion { get; }



    }
}
