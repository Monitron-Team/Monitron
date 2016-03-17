using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitron.DigitalOceanVmManagment
{
    public enum ActionType
    {
        StartVm,
        StopVm,
        DeleteVm,
        RebootVm,
        PowerCycleVm,
        ResetPasswordVm,
        ShutdownVm,
        DisableBackups,
        EnableIpv6,
        EnablePrivateNetworking
    }
}
