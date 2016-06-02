using System;
using System.Linq;

using Monitron.Plugins.LocalMonitorPlugin.Common;

using Docker.DotNet;
using Docker.DotNet.Models;

namespace Monitron.Plugins.LocalMonitorPlugin
{
    public class WorkerManager : IWorkerManager
    {
        public WorkerManager()
        {
            
        }

        public string[] GetRunningWorkerIds()
        {
            DockerClient client = new DockerClientConfiguration(new Uri("http://localhost:4243"))
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

