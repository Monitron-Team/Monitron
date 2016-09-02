using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Monitron.Common;

namespace Monitron.Node
{
	public sealed class Node
	{
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
		private Account m_Account;
		private NodeConfiguration m_NodeConfig;
		private IPluginDataStore m_DataStore;
		private readonly EventWaitHandle r_StopWaitHandle = new EventWaitHandle(initialState: false, mode: EventResetMode.ManualReset);
		public IMessengerClient MessangerClient { get; private set; }
		public INodePlugin Plugin { get; private set;}

		public Node(NodeConfiguration i_NodeConfig)
		{
			m_NodeConfig = i_NodeConfig;
			m_Account = i_NodeConfig.Account;
            loadMessangerClient();
            loadDataStore();
            loadPlugin();
        }

        private void loadMessangerClient()
        {
            sr_Log.Info("Loading messenger client...");
			Type type = loadDll(m_NodeConfig.MessageClient.DllPath, m_NodeConfig.MessageClient.DllName, 
				m_NodeConfig.MessageClient.Type);
            sr_Log.DebugFormat("Messenger client is {0}", type.FullName);
			MessangerClient = (IMessengerClient)Activator.CreateInstance(type, m_Account);
		}

        private void loadDataStore()
        {
            sr_Log.Info("Loading data store...");
            Type type = loadDll(m_NodeConfig.DataStore.DllPath, m_NodeConfig.DataStore.DllName, 
                m_NodeConfig.DataStore.Type);
            sr_Log.DebugFormat("Data store is {0}", type.FullName);
            m_DataStore = (IPluginDataStore)Activator.CreateInstance(type, m_NodeConfig.DataStore.Location);
        }

		private void loadPlugin()
		{
            sr_Log.Info("Loading plugin client...");
			Type type = loadDll(m_NodeConfig.Plugin.DllPath, m_NodeConfig.Plugin.DllName, m_NodeConfig.Plugin.Type);
            sr_Log.DebugFormat("Plugin client is {0}", type.FullName);
			Plugin = (INodePlugin)Activator.CreateInstance(type, MessangerClient, m_DataStore);
		}

		private Type loadDll(string i_DllPath, string i_DllName, string i_ClassType)
		{
			Type type = null;
			string fullDllPath = Path.Combine(i_DllPath, i_DllName);

			if (File.Exists(fullDllPath))
			{
				var Dll = Assembly.LoadFrom(fullDllPath);
				type = Dll.GetType(i_ClassType);
			}

            if (type == null)
            {
                throw new TypeLoadException("Could not find type");
            }

			return type;
		}

        public void Stop()
        {
            r_StopWaitHandle.Set();
        }

        public void Run()
        {
            sr_Log.Info("Running");
            r_StopWaitHandle.WaitOne();
        }
	}
}

