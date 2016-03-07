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
            string assemblyName = Assembly.GetCallingAssembly().FullName;
            return string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<NodeConfiguration>
  <Account>
    <Username>test_node_001</Username>
    <Domain>monitron.ddns.net</Domain>
    <Password>XXXX</Password>
  </Account>
  <Plugin>
    <DllPath>{0}</DllPath>
    <DllName>{1}</DllName>
    <Type>Monitron.Node.Tests.TestNodePlugin</Type>
    <AssemblyName>{{{2}}}</AssemblyName>
  </Plugin>
</NodeConfiguration>",
            assemblyDirectory,
            assemblyFileName,
            assemblyName);
        }
        
		[Test()]
		public void TestNodeConfigSerealizeDesrealize()
		{
			//Serialize
            MemoryStream stream = new MemoryStream();

			NodeConfiguration nodeTestConfig = new NodeConfiguration();
			nodeTestConfig.Account = new Monitron.Common.Account("user", "password", "domain");
			nodeTestConfig.DllPath = "path";
			nodeTestConfig.DllName = "Monitron.Node.Tests";
			nodeTestConfig.Plugin = "Plugin";
			nodeTestConfig.PluginAssemblyName = "Assembly";
            nodeTestConfig.Save(stream);
            //Deserialize
            stream.SetLength(stream.Position);
            stream.Position = 0;
            NodeConfiguration nodeLoadFromConfig = NodeConfiguration.Load(stream);
			Assert.AreEqual("user", nodeLoadFromConfig.Account.Identity.UserName);
			Assert.AreEqual("domain", nodeLoadFromConfig.Account.Identity.Domain);
			Assert.AreEqual("password", nodeLoadFromConfig.Account.Password);
			Assert.AreEqual("path", nodeLoadFromConfig.DllPath);
			Assert.AreEqual("Monitron.Node.Tests", nodeLoadFromConfig.DllName);
			Assert.AreEqual("Plugin", nodeLoadFromConfig.Plugin);
			Assert.AreEqual("Assembly", nodeLoadFromConfig.PluginAssemblyName);
		}

		[Test()]
		public void CreateNodeUsingNodeConfiguration()
		{
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(getTestConfig()));
            NodeConfiguration nodeConfig = NodeConfiguration.Load(stream);
            Node node = new Node(nodeConfig);
			Assert.AreEqual(typeof(Monitron.Node.Tests.TestNodePlugin).ToString(), node.Plugin.ToString());
		}
	}
}

