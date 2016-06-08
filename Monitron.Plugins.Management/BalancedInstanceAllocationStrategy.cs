using System;
using Monitron.Plugins.LocalMonitorPlugin.Common;
using System.Collections.Generic;

namespace Monitron.Plugins.Management
{
    public class BalancedInstanceAllocationStrategy : IInstanceAllocationStrategy
    {
        public BalancedInstanceAllocationStrategy()
        {
        }

        public IWorkerManager SelectWorkerForNewInstance(IEnumerable<IWorkerManager> i_AvailableManagers)
        {
            int min = int.MaxValue;
            IWorkerManager selectedManager = null;
            foreach (var manager in i_AvailableManagers)
            {
                int numInstances = manager.ListInstances().Statuses.Length;
                if (numInstances < min)
                {
                    selectedManager = manager;
                    min = numInstances;
                }
            }

            return selectedManager;
        }
    }
}

