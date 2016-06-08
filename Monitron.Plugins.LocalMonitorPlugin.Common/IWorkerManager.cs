﻿using System;

namespace Monitron.Plugins.LocalMonitorPlugin.Common
{
    public interface IWorkerManager
    {
        ListInstancesResult ListInstances();
        CreateInstanceResult CreateInstance(string i_Name, string i_PluginId, string i_Config);
        RemoveInstanceResult RemoveInstance(string i_Name);
    }
}

