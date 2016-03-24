using System.Collections.Generic;

namespace Monitron.CloudManagers
{
    public interface ICloudManager
    {
        IVirtualMachine CreateVm(VmCreationOptions i_Params);
        void DeleteVm(int i_VmId);
        void RebootVm(int i_VmId);
        void PowerOffVm(int i_VmId);
        void PowerOnVm(int i_VmId);
        IReadOnlyList<IVirtualMachine> GetVms();
        IVirtualMachine GetVm(int i_VmId);
        IReadOnlyList<string> GetSupportedImages();
        IReadOnlyList<string> GetSupportedSizes();
    }
}
