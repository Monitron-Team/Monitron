﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API;
using Monitron.VmManagment;
using Action = DigitalOcean.API.Models.Responses.Action;
using Image = DigitalOcean.API.Models.Responses.Action;

namespace Monitron.DigitalOceanVmManagment
{
    public class DigitalOceanVmManager : IVmManager
    {
        private readonly DigitalOceanClient r_DoClient;  //DigitalOcean Client

        public DigitalOceanVmManager(string i_Token)
        {
            r_DoClient = new DigitalOceanClient(i_Token);
        }

        public IVirtualMachine CreateVm(VmCreationParams i_Params)
        {
            var droplet = new DigitalOcean.API.Models.Requests.Droplet
            {
                Name = i_Params.Name,
                SizeSlug = i_Params.SizeSlug,
                Ipv6 = i_Params.Ipv6Enabled,
                RegionSlug = i_Params.RegionSlug,
                Backups = i_Params.BackupsEnabled,
                PrivateNetworking = i_Params.PrivateNetworkingEnabled,
                ImageIdOrSlug = i_Params.ImageIdOrSlug,
            };
            var response = this.r_DoClient.Droplets.Create(droplet); //Getting a new droplet from the DO server
            Response result = ExProccesResponse(response);
            IVirtualMachine newDeoplet = null;
            if (result.Succeded)
            {
                newDeoplet = new DropletWrapper(response.Result);  //creat a Droplet wrapper using the actual droplet
            }
            return newDeoplet;
        }

        public Response DeleteVm(int i_VmId)
        {
            Response result = new Response();
            var response = r_DoClient.Droplets.Delete(i_VmId); //Delete the droplet
            response.Wait();
            ExProccesResponse(response);
            if ((response.Status == TaskStatus.RanToCompletion))
            {
                result.Succeded = true;
            }
            else
            {
                result.Message = "Error Deleting the VM";
            }
            return result;
        }

        public Response PowerOnVm(int i_VmId)
        {
            Response response;
            if (GetVmById(i_VmId).Status != "active")       //check if the VM is not On
            {
                response = ExProccesResponse(this.r_DoClient.DropletActions.PowerOn(i_VmId));
            }
            else
            {
                response = new Response { Succeded = false, Message = "VM is already on" };
            }
            return response;
        }

        public Response PowerOffVm(int i_VmId)
        {
            Response response;
            if (GetVmById(i_VmId).Status != "off")      //check if the VM is not Off
            {
                var serverResponse = this.r_DoClient.DropletActions.PowerOff(i_VmId);
                serverResponse.Wait();  //to saggi - i didnt work without .Wait()... but why?!
                response = ExProccesResponse(serverResponse);
            }
            else
            {
                response = new Response { Succeded = false, Message = "VM is already off" };
            }
            return response;
        }

        public Response RebuildVm(int i_VmId, string i_Image)
        {
            Response result;
            var image = this.r_DoClient.Images.GetAll().Result.First(i_Item => i_Item.Slug == i_Image);
            if (image != null)   //if true then the server has the requsted image
            {
                Task<Action> response = this.r_DoClient.DropletActions.Rebuild(i_VmId, i_Image);
                result = ExProccesResponse(response);
            }
            else
            {
                result = new Response { Message = "Unknown Image", Succeded = false };
            }

            return result;
        }

        public Response RebootVm(int i_VmId)
        {
            Response response;
            var vm = GetVmById(i_VmId);
            if (vm.Status != "" && vm.Status != "off")          //check if VM is not already off
            {
                response = ExProccesResponse(this.r_DoClient.DropletActions.Reboot(i_VmId));
            }
            else
            {
                response = new Response { Message = "Requst Failed because VM is off", Succeded = false };
            }

            return (response);
        }

        public Response PowerCycleVm(int i_VmId)
        {
            var response = this.r_DoClient.DropletActions.PowerCycle(i_VmId);
            return ExProccesResponse(response);
        }

        public Response ResetPasswordVm(int i_VmId)
        {
            var response = this.r_DoClient.DropletActions.ResetPassword(i_VmId);
            return ExProccesResponse(response);
        }

        public Response ShutdownVm(int i_VmId)
        {
            var response = this.r_DoClient.DropletActions.Shutdown(i_VmId);
            return ExProccesResponse(response);
        }

        public Response DisableBackups(int i_VmId)
        {
            var response = this.r_DoClient.DropletActions.DisableBackups(i_VmId);
            return ExProccesResponse(response);
        }

        public Response RenameVm(int i_VmId, string i_NewName)
        {
            var response = this.r_DoClient.DropletActions.Rename(i_VmId, i_NewName);
            return ExProccesResponse(response);
        }

        public Response EnableIpv6(int i_VmId)
        {
            var response = this.r_DoClient.DropletActions.EnableIpv6(i_VmId);
            return ExProccesResponse(response);
        }

        public Response EnablePrivateNetworking(int i_VmId)
        {
            IVirtualMachine vm = GetVmById(i_VmId);
            Response response;
            bool vmIsOff = (vm.Status == "off");
            if (!vmIsOff)
            {
                var powerOffResponse = this.PowerOffVm(i_VmId);
                if (powerOffResponse.Succeded)
                {
                    vmIsOff = true;
                }
            }

            if (vmIsOff)
            {
                response = ExProccesResponse(this.r_DoClient.DropletActions.EnablePrivateNetworking(i_VmId));
            }
            else
            {
                response = new Response
                {
                    Message =
                               "Error executing \"EnablePrivateNetworking\" due to a probllem powering off the VM",
                    Succeded = false
                };
            }

            return response;
        }

        public int GetVmIdByName(string i_VmName, out bool o_Success)
        {
            int result = -1;
            var droplet = this.r_DoClient.Droplets.GetAll().Result.Where(i_Item => i_Item.Name == i_VmName);
            if (droplet.Count() != 0)
            {
                result = droplet.First().Id;
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
            Response result = new Response();
            if ((i_ActionServerResponse.Status == TaskStatus.RanToCompletion))
            {
                result.Succeded = true;
                result.Message = "RanToCompletion";
            }
            else
            {
                result.Message = i_ActionServerResponse.Status.ToString(); // "Error"; This is not informative engough
            }
            return result;
        }

        public IList<string> GetSupportedImages()
        {
            var allImages = this.r_DoClient.Images.GetAll().Result;

            return allImages.Select(image => image.Slug).ToList();
        }

        public IList<string> GetSupportedSizes()
        {
            List<string> result = new List<string>();
            var sizes = this.r_DoClient.Sizes.GetAll().Result;
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
                Task<Action> response = this.r_DoClient.DropletActions.Resize(i_VmId, "1gb"/*i_NewSize*/);
                result = ExProccesResponse(response);
            }
            else
            {
                result = new Response { Message = "Unsupported size", Succeded = false };
            }

            return result;
        }

        public IVirtualMachine GetVmById(int i_VmId)
        {
            DropletWrapper vm = null;
            var droplet = this.r_DoClient.Droplets.Get(i_VmId).Result;
            /*
            var droplets = m_DoClient.Droplets.GetAll().Result.Where(i_Item => i_Item.Id == i_VmId);
            if (droplets.Count() > 0)
            {
                VM = new DropletWrapper(droplets.First());
            }
            */
            if (droplet != null)
            {
                vm = new DropletWrapper(droplet);
            }

            return vm;
        }
    }
}
