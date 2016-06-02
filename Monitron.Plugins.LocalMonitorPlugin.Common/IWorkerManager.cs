using System;

namespace Monitron.Plugins.LocalMonitorPlugin.Common
{
    public interface IWorkerManager
    {
        string[] GetRunningWorkerIds();
    }
}

