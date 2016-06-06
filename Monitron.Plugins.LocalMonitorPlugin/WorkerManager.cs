using System;
using System.Linq;

using Monitron.Plugins.LocalMonitorPlugin.Common;

using Docker.DotNet;
using Docker.DotNet.Models;
using Monitron.Common;
using Monitron;
using System.IO;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace Monitron.Plugins.LocalMonitorPlugin
{
    public class WorkerManager : IWorkerManager
    {
        private readonly StoredPluginsManager r_PluginsManager;
        
        public WorkerManager(StoredPluginsManager i_PluginsManager)
        {
            r_PluginsManager = i_PluginsManager;
        }

        private DockerClient createClient()
        {
            return new DockerClientConfiguration(new Uri("http://localhost:2376"))
                .CreateClient();
        }

        public async Task<CreateInstanceResult> CreateInstanceAsync(
            string i_Name,
            string i_PluginId,
            string i_Config)
        {
            Stream pluginStream;
            try
            {
                pluginStream = r_PluginsManager.OpenPluginDownloadStream(i_PluginId);
            }
            catch (Exception e)
            {
                return new CreateInstanceResult
                {
                    Success = false,
                    Error = string.Format("Could not find plugin '{0}'", i_PluginId)
                };
            }
            
            DockerClient client = createClient();
            string containerId;
            try
            {
                var resp = await client.Containers.CreateContainerAsync(
                new CreateContainerParameters
                {
                    ContainerName = i_Name,
                    Config = new Config
                        {
                            Image = "monitron/node-container",
                            Tty = false,
                        },
                });

                containerId = resp.Id;
            }
            catch (Exception e)
            {
                return new CreateInstanceResult
                {
                    Success = false,
                    Error = string.Format("Could not create container: {0}", e.Message)
                };
            }

            bool containerStarted = await client.Containers.StartContainerAsync(
                                        containerId,
                                        new HostConfig
                {
                    Privileged = false,
                });

            if (!containerStarted)
            {
                await client.Containers.RemoveContainerAsync(
                    containerId,
                    new RemoveContainerParameters
                    {
                        Force = true,
                        RemoveVolumes = true,
                    });
                
                return new CreateInstanceResult
                {
                    Success = false,
                    Error = string.Format("Could not start container")
                };
            }

            try
            {
                await client.Containers.ExtractArchiveToContainerAsync(
                    containerId,
                    new ExtractArchiveToContainerParameters
                    {
                        Path = "/opt/Node",
                    },
                    pluginStream,
                    new System.Threading.CancellationToken());
            }
            catch (Exception e)
            {
                await client.Containers.RemoveContainerAsync(
                    containerId,
                    new RemoveContainerParameters
                    {
                        Force = true,
                        RemoveVolumes = true,
                    });

                return new CreateInstanceResult
                {
                    Success = false,
                    Error = string.Format("Could not upload plugin to container: {0}", e.Message)
                };
            }

            try
            {
                var ms = new MemoryStream();
                var tar = new TarOutputStream(ms);
                var encoder = new System.Text.UTF8Encoding();
                var conf = encoder.GetBytes(i_Config);
                var entry = TarEntry.CreateTarEntry("/opt/Node/node.conf");
                entry.Size = conf.Length;
                tar.PutNextEntry(entry);
                tar.Write(conf, 0, conf.Length);
                tar.CloseEntry();
                tar.Close();
                ms = new MemoryStream(ms.GetBuffer());
                await client.Containers.ExtractArchiveToContainerAsync(
                    containerId,
                    new ExtractArchiveToContainerParameters
                    {
                        Path = "/",
                    },
                    ms,
                    new System.Threading.CancellationToken());
            }
            catch(Exception e)
            {
                await client.Containers.RemoveContainerAsync(
                    containerId,
                    new RemoveContainerParameters
                    {
                        Force = true,
                        RemoveVolumes = true,
                    });

                return new CreateInstanceResult
                {
                    Success = false,
                    Error = string.Format("Could not upload configuration to container: {0}", e.Message)
                };
            }

            return new CreateInstanceResult
            {
                Success = true,
                Error = string.Empty,
            };
        }


        public CreateInstanceResult CreateInstance(string i_Name, string i_PluginId, string i_Config)
        {
            return CreateInstanceAsync(i_Name, i_PluginId, i_Config).Result;
        }

        public async Task<RemoveInstanceResult> RemoveInstanceAsync(string i_Name)
        {
            DockerClient client = createClient();
            try
            {
                await client.Containers.RemoveContainerAsync(
                    i_Name,
                    new RemoveContainerParameters
                    {
                        Force = true,
                        RemoveVolumes = true,
                    });
            }
            catch (Exception e)
            {
                return new RemoveInstanceResult
                {
                    Success = false,
                    Error = e.Message,
                };
            }

            return new RemoveInstanceResult
            {
                Success = true,
                Error = string.Empty,
            };
        }

        public RemoveInstanceResult RemoveInstance(string i_Name)
        {
            return RemoveInstanceAsync(i_Name).Result;
        }

        public string[] GetRunningWorkerIds()
        {
            DockerClient client = new DockerClientConfiguration(new Uri("http://localhost:2376"))
                .CreateClient();
            return client.Containers.ListContainersAsync(
                new ListContainersParameters
                {
                    All = true,
                })
                .Result
                .Select((container) => container.Names.First())
                .ToArray();
        }
    }
}

