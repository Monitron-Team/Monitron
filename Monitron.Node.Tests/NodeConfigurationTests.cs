using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;
using NUnit.Framework;

using Monitron.Node;
using System.Text;

namespace Monitron.Node.Tests
{
	[TestFixture()]
	public class NodeConfigurationTests
	{
        public string getTestConfig()
        {
            string assemblyLocation = Assembly.GetCallingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            string assemblyFileName = Path.GetFileName(assemblyLocation);
            return string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<NodeConfiguration>
  <MessangerClient>
	<DllPath>{0}</DllPath>
	<DllName>{1}</DllName>
	<Type>Monitron.Node.Tests.TestClientMessanger</Type>
  </MessangerClient>
  <Account>
    <Username>test_node_001</Username>
    <Domain>monitron.ddns.net</Domain>
    <Password>XXXX</Password>
  </Account>
  <Plugin>
    <DllPath>{0}</DllPath>
    <DllName>{1}</DllName>
    <Type>Monitron.Node.Tests.TestNodePlugin</Type>
  </Plugin>
</NodeConfiguration>",
            assemblyDirectory,
            assemblyFileName);
        }
        
		[Test()]
		public void TestNodeConfigSerealizeDesrealize()
		{
			//Serialize
            MemoryStream stream = new MemoryStream();

			NodeConfiguration nodeTestConfig = new NodeConfiguration();
			nodeTestConfig.Account = new Monitron.Common.Account("user", "password", "domain");
			nodeTestConfig.Plugin.DllPath = "PluginPath";
			nodeTestConfig.Plugin.DllName = "Monitron.Node.Tests";
			nodeTestConfig.Plugin.Type = "PluginType";
			nodeTestConfig.MessageClient.DllPath = "MessageClientPath";
			nodeTestConfig.MessageClient.DllName = "MessageClientTest";
			nodeTestConfig.MessageClient.Type = "MessageClientType";
            nodeTestConfig.Save(stream);
            //Deserialize
            stream.SetLength(stream.Position);
            stream.Position = 0;
            NodeConfiguration nodeLoadFromConfig = NodeConfiguration.Load(stream);
			Assert.AreEqual("user", nodeLoadFromConfig.Account.Identity.UserName);
			Assert.AreEqual("domain", nodeLoadFromConfig.Account.Identity.Domain);
			Assert.AreEqual("password", nodeLoadFromConfig.Account.Password);
			Assert.AreEqual("PluginPath", nodeLoadFromConfig.Plugin.DllPath);
			Assert.AreEqual("Monitron.Node.Tests", nodeLoadFromConfig.Plugin.DllName);
			Assert.AreEqual("PluginType", nodeLoadFromConfig.Plugin.Type);
			Assert.AreEqual("MessageClientPath", nodeLoadFromConfig.MessageClient.DllPath);
			Assert.AreEqual("MessageClientTest", nodeLoadFromConfig.MessageClient.DllName);
			Assert.AreEqual("MessageClientType", nodeLoadFromConfig.MessageClient.Type);
		}

		[Test()]
		public void CreateNodeUsingNodeConfiguration()
		{
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(getTestConfig()));
            NodeConfiguration nodeConfig = NodeConfiguration.Load(stream);
            Node node = new Node(nodeConfig);
			Assert.AreEqual(typeof(Monitron.Node.Tests.TestNodePlugin).ToString(), node.Plugin.ToString());
			Assert.AreEqual(typeof(Monitron.Node.Tests.TestClientMessanger).ToString(), node.MessangerClient.ToString());
		}
	}
}

