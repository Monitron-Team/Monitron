using System;
using Monitron.Common;

namespace Monitron.Node.Tests
{
	public sealed class TestNodePlugin: INodePlugin
	{
		public IMessengerClient MessangerClient { get; private set; }

		public TestNodePlugin(IMessengerClient i_MessangerClient)
		{
			MessangerClient = i_MessangerClient;
		}
	}
}

