using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalOcean.API;
using Monitron.VmManagment;
using DigitalOcean.API.Models.Requests;
using DigitalOcean.API.Models.Responses;

using Action = DigitalOcean.API.Models.Responses.Action;

///using Action = System.Action;

namespace Monitron.DigitalOceanVmManagment
{
    public class DigitalOceanVmManager : IVmManager
    {
        public List<string> debug = new List<string>(); 
        private DigitalOceanClient m_DoClient;
        ///private Dictionary<int, IVirtualMachine> m_VMInventory;

        public DigitalOceanVmManager(string i_Token)
        {
            m_DoClient = new DigitalOceanClient(i_Token);
            var dd = m_DoClient.Droplets.GetAll();
            apitest(m_DoClient);

            //m_VMInventory = new Dictionary<int, IVirtualMachine>();
        }

        public async void apitest(DigitalOceanClient client)
        {
            var droplets = await client.Droplets.GetAll();
            int count = droplets.Count;
            count++;
        }

        public IVirtualMachine CreateVM(VmCreationParams i_Params)
        {

            var droplet = new DigitalOcean.API.Models.Requests.Droplet
            {
                             Name = i_Params.Name,
                             SizeSlug = i_Params.SizeSlug,
                             Ipv6 = i_Params.Ipv6,
                             RegionSlug = i_Params.RegionSlug,
                             Backups = i_Params.Backups,
                             PrivateNetworking = i_Params.PrivateNetworking,
                             ImageIdOrSlug = i_Params.ImageIdOrSlug,
                             SshIdsOrFingerprints = i_Params.SshIdsOrFingerprints,
                         };

            //var droplets = this.m_DoClient.Droplets.GetAll();
            this.m_DoClient.Images.GetAll();
            this.m_DoClient.Sizes.GetAll();
            var Response = this.m_DoClient.Droplets.Create(droplet);

            for (int i = 0; i < 5000; i++)
            {
                debug.Add(Response.Status.ToString());
               // Console.WriteLine(Response.Status.ToString());
            }
            
            IVirtualMachine newDeoplet = null;
            Thread.Sleep(10000);  //maybe not nessecary
            if (Response.IsCompleted)
                if (!Response.IsFaulted)
                {
                    newDeoplet = new DropletWrapper(Response.Result);
                    //m_VMInventory.Add(Response.Result.Id, newDeoplet);
                }
                else
                {
                    //falted
                }
            else if (Response.IsCanceled)
            {
                //canceled
            }

            return newDeoplet;  //should i return status by params?
        }

        public bool DeleteVm(int i_VmId)
        {
            var Response = m_DoClient.Droplets.Delete(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool StartVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerOn(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool StopVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.Reboot(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool RebuildVm(int i_VmId, string i_Image)
        {
            var Response = m_DoClient.DropletActions.Rebuild(i_VmId, i_Image);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool RebootVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.Reboot(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);   
        }

        public bool PowerCycleVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerCycle(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool ResetPasswordVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.ResetPassword(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool ShutdownVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerOff(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool DisableBackups(int i_VmId)
        {
            throw new NotImplementedException();
        }

        public bool EnableIpv6(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.EnableIpv6(i_VmId);
            Response.Wait();
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool EnablePrivateNetworking(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.EnablePrivateNetworking(i_VmId);
            Response.Wait();
            //var Response = m_DoClient.DropletActions.EnablePrivateNetworking(i_VmId);
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public int GetVmIdByName(string i_Name, out bool o_Success)
        {
            int result = -1;
            var Droplet = this.m_DoClient.Droplets.GetAll().Result.Where(item => item.Name == i_Name).First();
            if (Droplet != null)
            {
                result = Droplet.Id;
                o_Success = true;
            }
            else
            {
                o_Success = false;
            }
            return result;
        }
    }
}
