using System;

namespace Monitron.Plugins.LocalMonitorPlugin.Common
{
    public struct ListInstancesResult
	{
        public bool Success;
        public string Error;
        public InstanceStatus[] Statuses;
	}

}

