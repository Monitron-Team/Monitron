using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DigitalOcean.API;
using DigitalOcean.API.Models.Requests;
using Monitron.CloudManagers;
using Action = DigitalOcean.API.Models.Responses.Action;

namespace Monitron.CloudManagers.DigitalOcean
{
    public class DigitalOceanCloudManager : ICloudManager
    {
        private readonly DigitalOceanClient r_DoClient;  //DigitalOcean Client

        public DigitalOceanCloudManager(string i_Token)
        {
            r_DoClient = new DigitalOceanClient(i_Token);
        }

        public IVirtualMachine CreateVm(VmCreationOptions i_Options)
        {
            var droplet = new Droplet
            {
                Name = i_Options.Name,
                SizeSlug = i_Options.SizeSlug,
                Ipv6 = i_Options.Ipv6Enabled,
                RegionSlug = i_Options.Region,
                Backups = false,
                PrivateNetworking = true,
                ImageIdOrSlug = i_Options.ImageIdOrSlug,
            };
            return new DropletWrapper(this.r_DoClient.Droplets.Create(droplet).Result);
        }

        public void DeleteVm(int i_VmId)
        {
            r_DoClient.Droplets.Delete(i_VmId).Wait();
        }

        public void RebootVm(int i_VmId)
        {
            // TODO: Try graceful reboot first
            r_DoClient.DropletActions.PowerCycle(i_VmId).Wait();
        }

        public void PowerOffVm(int i_VmId)
        {
            // TODO: Add gracefull shutdown
            r_DoClient.DropletActions.PowerOff(i_VmId).Wait();
        }

        public void PowerOnVm(int i_VmId)
        {
            r_DoClient.DropletActions.PowerOn(i_VmId).Wait();
        }

        public IReadOnlyList<IVirtualMachine> GetVms()
        {
            return r_DoClient.Droplets.GetAll().Result.Select(
                item => new DropletWrapper(item)
            ).ToList();
        }

        public IVirtualMachine GetVm(int i_VmId)
        {
            return new DropletWrapper(r_DoClient.Droplets.Get(i_VmId).Result);
        }

        public IReadOnlyList<string> GetSupportedImages()
        {
            return r_DoClient.Images.GetAll().Result.Select(
                item => item.Slug
            ).ToList();
        }

        public IReadOnlyList<string> GetSupportedSizes()
        {
            return r_DoClient.Sizes.GetAll().Result.Select(
                item => item.Slug
            ).ToList();
        }
    }
}

