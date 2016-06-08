using System;

namespace Monitron.Plugins.LocalMonitorPlugin.Common
{
    public interface IWorkerManager
    {
        ListInstancesResult ListInstances();
        CreateInstanceResult CreateInstance(string i_Name, string i_PluginId, string i_Config);
        UnpauseInstanceResult UnpauseInstance(string i_Name);
        PauseInstanceResult PauseInstance(string i_Name);
        RemoveInstanceResult RemoveInstance(string i_Name);
        GetLogResult GetLog(string i_Name);
    }
}

