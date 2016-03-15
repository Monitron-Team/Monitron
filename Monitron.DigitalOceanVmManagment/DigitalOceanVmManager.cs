using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using Monitron.VmManagment;
using DigitalOcean.API.Models.Requests;

using Action = System.Action;

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

            var pp = new Droplet
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

            var droplets = this.m_DoClient.Droplets.GetAll();
            this.m_DoClient.Images.GetAll();
            this.m_DoClient.Sizes.GetAll();
            var Response = this.m_DoClient.Droplets.Create(pp);

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
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool StartVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerOn(i_VmId);
            return (Response.Status == TaskStatus.RanToCompletion);
        }

        public bool StopVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerOff(i_VmId);
            return (Response.Status == TaskStatus.RanToCompletion);
        }

      

        public bool RebootVm(int i_VmId)
        {
            bool result = false;
            var Response = m_DoClient.DropletActions.PowerOn(i_VmId);
            return (Response.Status == TaskStatus.RanToCompletion);   
        }

        public bool PowerCycleVm(int i_VmId)
        {
            throw new NotImplementedException();
        }

        public bool ResetPasswordVm(int i_VmId)
        {
            throw new NotImplementedException();
        }

        public bool ShutdownVm(int i_VmId)
        {
            throw new NotImplementedException();
        }

        public bool DisableBackups(int i_VmId)
        {
            throw new NotImplementedException();
        }

        public bool EnableIpv6(int i_VmId)
        {
            throw new NotImplementedException();
        }

        public Task<Action> EnablePrivateNetworking(int dropletId)
        {
            throw new NotImplementedException();
        }
    }
}
