using System;
using Monitron.Plugins.LocalMonitorPlugin.Common;
using System.Collections.Generic;

namespace Monitron.Plugins.Management
{
    public interface IInstanceAllocationStrategy
    {
        IWorkerManager SelectWorkerForNewInstance(IEnumerable<IWorkerManager> i_AvailableManagers);
    }
}

