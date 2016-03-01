using System;
using System.Reflection;
using Monitron.Common;
using Monitron.Clients.XMPP;

using System.Linq;


namespace Monitron.Node
{
	public sealed class Node
	{
		private Account m_Account;
		private IMessengerClient m_MessangerClient;
		private IPlugin m_Plugin;
		//private IDataStore m_DataStore;


		public Node(NodeConfiguration i_NodeConfig)
		{
			m_Account = i_NodeConfig.Account;
			m_MessangerClient = new XMPPMessengerClient(m_Account);
			try
			{
				Type type = Type.GetType(i_NodeConfig.Plugin + ", " + i_NodeConfig.PluginAssemblyName);
				if (type!=null)
				{
					m_Plugin = (IPlugin)Activator.CreateInstance (type, m_MessangerClient);
				}
			}
			catch(TypeLoadException e)
			{
				throw new TypeLoadException ("Could not load plugin " + i_NodeConfig.Plugin, e);
			}

		}
	}
}

