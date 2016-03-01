using NUnit.Framework;
using System;
using Monitron.Node;

namespace Monitron.Node.Tests
{
	[TestFixture ()]
	public class Test
	{
		[Test ()]
		public void LoadConfigurationAndConnectClient ()
		{
			NodeConfiguration nodeConfig = new NodeConfiguration();
			nodeConfig.LoadConfigFromFile ("node.config");
			Node node = new Node (nodeConfig);
		}
	}
}

