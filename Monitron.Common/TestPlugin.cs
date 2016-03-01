using System;

namespace Monitron.Common
{
	public sealed class TestPlugin: IPlugin
	{
		IMessengerClient m_MessangerClient;
		public IMessengerClient MessangerClient
		{
			get {return m_MessangerClient;}
		}

		public TestPlugin (IMessengerClient i_MessangerClient)
		{
			m_MessangerClient = i_MessangerClient;
		}
	}
}

