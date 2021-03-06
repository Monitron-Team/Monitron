﻿using System;
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
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
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
            Stream pluginStream = null;
            try
            {
                pluginStream = r_PluginsManager.OpenPluginDownloadStream(i_PluginId);
            }
            catch
            {
            }

            if (pluginStream == null)
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
						Name = i_Name,
                        Image = "monitron/node-container",
                        Tty = false,
                        HostConfig = new HostConfig {
                            AutoRemove = true,
                        }
                    });
                
				containerId = resp.ID;
            }
            catch (Exception e)
            {
                sr_Log.Error(e);
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
                    AutoRemove = true,
                });

            if (!containerStarted)
            {
                await client.Containers.RemoveContainerAsync(
                    containerId,
					new ContainerRemoveParameters
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
					new ContainerPathStatParameters
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
					new ContainerRemoveParameters
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
                    new ContainerPathStatParameters
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
                    new ContainerRemoveParameters
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
					new ContainerRemoveParameters
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

        public async Task<UnpauseInstanceResult> UnpauseInstanceAsync(string i_Name)
        {
            try
            {
                var client = createClient();
                await client.Containers.UnpauseContainerAsync(i_Name);
            }
            catch (Exception e)
            {
                return new UnpauseInstanceResult
                {
                    Success = false,
                    Error = e.Message,
                };
            }

            return new UnpauseInstanceResult
            {
                Success = true,
                Error = string.Empty,
            };
        }

        public GetLogResult GetLog(string i_Name)
        {
            return GetLogAsync(i_Name).Result;
        }

        public async Task<GetLogResult> GetLogAsync(string i_Name)
        {
            try
            {
                var client = createClient();
                var sr = new StreamReader(await client.Containers.GetContainerLogsAsync(
                    i_Name,
					new ContainerLogsParameters
                    {
                        Follow = false,
						ShowStderr = true,
						ShowStdout = true,
						Timestamps = false,
                    },
                    new System.Threading.CancellationToken()
                ));
                return new GetLogResult
                {
                    Success = true,
                    Error = string.Empty,
                    Log = sr.ReadLine(),
                };
            }
            catch (Exception e)
            {
                return new GetLogResult
                {
                    Success = false,
                    Error = e.Message,
                    Log = string.Empty,
                };
            }
        }


        public UnpauseInstanceResult UnpauseInstance(string i_Name)
        {
            return UnpauseInstanceAsync(i_Name).Result;
        }


        public async Task<PauseInstanceResult> PauseInstanceAsync(string i_Name)
        {
            try
            {
                var client = createClient();
                await client.Containers.PauseContainerAsync(i_Name);
            }
            catch (Exception e)
            {
                return new PauseInstanceResult
                {
                    Success = false,
                    Error = e.Message,
                };
            }

            return new PauseInstanceResult
            {
                Success = true,
                Error = string.Empty,
            };
        }

        public PauseInstanceResult PauseInstance(string i_Name)
        {
            return PauseInstanceAsync(i_Name).Result;
        }

        public RemoveInstanceResult RemoveInstance(string i_Name)
        {
            return RemoveInstanceAsync(i_Name).Result;
        }

        public ListInstancesResult ListInstances()
        {
            var client = createClient();
            try
            {
            return new ListInstancesResult
            {
                Success = true,
                Error = string.Empty,
                Statuses = client.Containers
						.ListContainersAsync(new ContainersListParameters
                    {
                        All = true,
                    })
                    .Result
                    .Select((container) =>
                    {
                        return new InstanceStatus
                        {
                            Name = container.Names.First().Substring(1),
                            Status = container.Status,
                        };
                    })
                    .ToArray(),
            };
            }
            catch (Exception e)
            {
                return new ListInstancesResult
                {
                    Success = false,
                    Error = e.Message,
                    Statuses = new InstanceStatus[0],
                };
            }
        }
    }
}

