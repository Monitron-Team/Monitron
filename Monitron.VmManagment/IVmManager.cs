using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitron.VmManagment
{
    public interface IVmManager
    {
        IVirtualMachine CreateVM(VmCreationParams i_Params);
        bool StartVm(int i_VmId);
        bool StopVm(int i_VmId);
        bool DeleteVm(int i_VmId);
        bool RebootVm(int i_VmId);
        bool PowerCycleVm(int i_VmId);
        bool ResetPasswordVm(int i_VmId);
        bool ShutdownVm(int i_VmId);
        bool DisableBackups(int i_VmId);
        bool EnableIpv6(int i_VmId);
        Task<Action> EnablePrivateNetworking(int dropletId);
        
        
        //bool ChangeKernel(int dropletId, int kernelId);
        //bool GetDropletAction(int dropletId, int actionId);
        //bool Rebuild(int dropletId, object imageIdOrSlug);
        //bool Rename(int dropletId, string name);
        //bool Resize(int dropletId, string sizeSlug);
        //bool Restore(int dropletId, int imageId);

        //Task<Action> Snapshot(int dropletId, string name);
    }
}
