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
        Response PowerOnVm(int i_VmId);
        Response PowerOffVm(int i_VmId);
        Response DeleteVm(int i_VmId);
        Response RebootVm(int i_VmId);
        Response PowerCycleVm(int i_VmId);
        Response ResetPasswordVm(int i_VmId);
        Response ShutdownVm(int i_VmId);
        Response DisableBackups(int i_VmId);
        Response EnableIpv6(int i_VmId);
        Response EnablePrivateNetworking(int dropletId);
        Response RebuildVm(int i_VmId, string i_Image);
        Response ResizeVm(int i_VmName, string i_NewSize);
        int GetVmIdByName(string i_VmName, out bool o_Success);
        Response RenameVm(int i_VmId, string i_NewName);
        IList<string> GetSupportedImages();
        IList<string> GetSupportedSizes();

        //More possebilities:
        //bool ChangeKernel(int dropletId, int kernelId);
        //bool GetDropletAction(int dropletId, int actionId);
        //bool Restore(int dropletId, int imageId);
        //Task<Action> Snapshot(int dropletId, string name);

    }
}
