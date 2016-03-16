using System;
using Monitron.Common;

namespace Monitron.Node.Tests
{
	public sealed class TestNodePlugin: INodePlugin
	{
		public IMessengerClient MessangerClient { get; private set; }
		public IPluginDataStore DataStore { get; private set;}

		public TestNodePlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
		{
			MessangerClient = i_MessangerClient;
			DataStore = i_DataStore;
		}
	}
}

