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
		private Account m_Account;
		private NodeConfiguration m_NodeConfig;
		private readonly EventWaitHandle r_StopWaitHandle = new EventWaitHandle(initialState: false, mode: EventResetMode.ManualReset);
		public IMessengerClient MessangerClient { get; private set; }
		public INodePlugin Plugin { get; private set;}

		public Node(NodeConfiguration i_NodeConfig)
		{
			m_NodeConfig = i_NodeConfig;
			m_Account = i_NodeConfig.Account;

			try
			{
				loadMessangerClient();
			}
			catch(TypeLoadException) 
			{
				MessangerClient = null;
			}
			try
			{
				loadPlugin();
			}
			catch(TypeLoadException e)
			{
				throw new TypeLoadException("Could not load plugin " + i_NodeConfig.Plugin.Type, e);
			}
		}

		private void loadMessangerClient()
		{
			Type type = loadDll(m_NodeConfig.MessageClient.DllPath, m_NodeConfig.MessageClient.DllName, 
				m_NodeConfig.MessageClient.Type);

			if (type != null)
			{
				MessangerClient = (IMessengerClient)Activator.CreateInstance(type, m_Account);
			}
		}

		private void loadPlugin()
		{
			Type type = loadDll(m_NodeConfig.Plugin.DllPath, m_NodeConfig.Plugin.DllName, m_NodeConfig.Plugin.Type);

			if (type != null)
			{
				Plugin = (INodePlugin)Activator.CreateInstance(type, MessangerClient);
			}
		}

		private Type loadDll(string i_DllPath, string i_DllName, string i_ClassType)
		{
			Type type = null;
			string fullDllPath = Path.Combine(i_DllPath, i_DllName);

			if (File.Exists(fullDllPath))
			{
				var Dll = Assembly.LoadFile(fullDllPath);
				type = Dll.GetType(i_ClassType);
			}

			return type;
		}

        public void Stop()
        {
            r_StopWaitHandle.Set();
        }

        public void Run()
        {
            r_StopWaitHandle.WaitOne();
        }
	}
}

