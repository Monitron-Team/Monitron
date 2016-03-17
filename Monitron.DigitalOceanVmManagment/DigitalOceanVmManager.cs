using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalOcean.API;
using Monitron.VmManagment;
using DigitalOcean.API.Models.Requests;
using DigitalOcean.API.Models.Responses;

using Action = DigitalOcean.API.Models.Responses.Action;
using Image = DigitalOcean.API.Models.Responses.Action;
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
            this.m_DoClient.Images.GetAll(); //todo: check if needed
            this.m_DoClient.Sizes.GetAll();  //todo: check if needed
            var Response = this.m_DoClient.Droplets.Create(droplet);
            Response Result = ExProccesResponse(Response);
            IVirtualMachine newDeoplet = null;
            if (Result.Succeded)
            {
                newDeoplet = new DropletWrapper(Response.Result);
            }
            return newDeoplet;  //should i return status by params?
        }

        public Response DeleteVm(int i_VmId)
        {
            Response Result = new Response ();
            var Response = m_DoClient.Droplets.Delete(i_VmId);
            Response.Wait();
            ExProccesResponse(Response);
            if ((Response.Status == TaskStatus.RanToCompletion))
            {
                Result.Succeded = true;
            }
            else
            {
                Result.Message = "Error";
            }
            return Result;
        }

        public Response PowerOnVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerOn(i_VmId);
            return ExProccesResponse(Response);
        }

        public Response PowerOffVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerOff(i_VmId);
            Response.Wait();
            return ExProccesResponse(Response);
        }

        public Response RebuildVm(int i_VmId, string i_Image)
        {
            Response result;
            var image = this.m_DoClient.Images.GetAll().Result.Where(item => item.Slug == i_Image).First();
            if (image != null)
            {
                Task<Action> Response = m_DoClient.DropletActions.Rebuild(i_VmId, i_Image);
                result = ExProccesResponse(Response);
            }
            else
            {
                result = new Response {Message = "Unknown Imamge",  Succeded = false};
            }

            return result;
        }

        public Response RebootVm(int i_VmId)
        {
            string status = "";
            Response Response;
            try
            {
                status = this.m_DoClient.Droplets.Get(i_VmId).Result.Status;
            }
            catch (Exception)
            {
                Response = new Response { Message = "Invalid Id", Succeded = false };
            }
            if (status != "" && status != "off")
            {
                Response = ExProccesResponse(m_DoClient.DropletActions.Reboot(i_VmId));
            }
            else
            {
                Response = new Response { Message = "Requst Failed because VM is off", Succeded = false };
            }
           
            return (Response);
        }

        public Response PowerCycleVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.PowerCycle(i_VmId);
            return ExProccesResponse(Response);
        }

        public Response ResetPasswordVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.ResetPassword(i_VmId);
            return ExProccesResponse(Response);
        }

        public Response ShutdownVm(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.Shutdown(i_VmId);
            return ExProccesResponse(Response);
        }

        public Response DisableBackups(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.DisableBackups(i_VmId);
            return ExProccesResponse(Response);
        }

        public Response RenameVm(int i_VmId, string i_NewName)
        {
            var Response = m_DoClient.DropletActions.Rename(i_VmId,i_NewName);
            return ExProccesResponse(Response);
        }

        public Response EnableIpv6(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.EnableIpv6(i_VmId);
            return ExProccesResponse(Response);
        }

        public Response EnablePrivateNetworking(int i_VmId)
        {
            var Response = m_DoClient.DropletActions.EnablePrivateNetworking(i_VmId);
            return ExProccesResponse(Response);
        }

        public int GetVmIdByName(string i_VmName, out bool o_Success)
        {
            int result = -1;
            var Droplet = m_DoClient.Droplets.GetAll().Result.Where(item => item.Name == i_VmName);
            if (Droplet.Count() != 0)
            {
                result = Droplet.First().Id;
                o_Success = true;
            }
            else
            {
                o_Success = false;
            }
            return result;
        }

        public Response ExProccesResponse(Task i_ActionServerResponse)
        {
            i_ActionServerResponse.Wait();
            Response Result = new Response();
            if ((i_ActionServerResponse.Status == TaskStatus.RanToCompletion))
            {
                Result.Succeded = true;
            }
            else
            {
                Result.Message = "Error";
            }
            return Result;
        }

        public IList<string> GetSupportedImages()
        {
            List<string> result = new List<string>();
            var AllImages = m_DoClient.Images.GetAll().Result;
            foreach (var image in AllImages)
            {
                result.Add(image.Slug);
            }

            return result;
        }

        public IList<string> GetSupportedSizes()
        {
            List<string> result = new List<string>();
            var sizes = m_DoClient.Sizes.GetAll().Result;
            foreach (var size in sizes)
            {
                result.Add(size.Slug);
            }

            return result;
        }

        public Response ResizeVm(int i_VmId, string i_NewSize)
        {
            Response result;
            var sizes = this.GetSupportedSizes();

            if (sizes.Contains(i_NewSize))
            {
                //Task<Action> response = m_DoClient.DropletActions.Resize(i_VmId, i_NewSize);
                var response = m_DoClient.DropletActions.Resize(i_VmId, i_NewSize).Result;
                var dd = response.Status;
                result = null;
                //result = ExProccesResponse(response);
            }
            else
            {
                result = new Response { Message = "Unsupported size", Succeded = false };
            }

            return result;
        }
    }
}
