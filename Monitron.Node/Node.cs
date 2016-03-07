using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using Monitron.Common;
using Monitron.Clients.XMPP;

namespace Monitron.Node
{
	public sealed class Node
	{
		private Account m_Account;
		private IMessengerClient m_MessangerClient;
		private NodeConfiguration m_NodeConfig;
        private readonly EventWaitHandle r_StopWaitHandle = new EventWaitHandle(initialState: false, mode: EventResetMode.ManualReset);
		public INodePlugin Plugin { get; private set;}

		public Node(NodeConfiguration i_NodeConfig)
		{
			m_NodeConfig = i_NodeConfig;
			m_Account = i_NodeConfig.Account;
			try
			{
				m_MessangerClient = new XMPPMessengerClient(m_Account);
			}
			catch(Exception) 
			{
				m_MessangerClient = null;
			}
			try
			{
				loadPlugin();
			}
			catch(TypeLoadException e)
			{
				throw new TypeLoadException("Could not load plugin " + i_NodeConfig.Plugin, e);
			}
		}

		private void loadPlugin()
		{
			string fullDllPath = Path.Combine(m_NodeConfig.DllPath, m_NodeConfig.DllName);

			if (File.Exists(fullDllPath))
			{
				var Dll = Assembly.LoadFile(fullDllPath);
				string classToLoad = m_NodeConfig.Plugin;
				Type type = Dll.GetType(classToLoad);

				if (type!=null)
				{
					Plugin = (INodePlugin)Activator.CreateInstance(type, m_MessangerClient);
				}
			}
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

