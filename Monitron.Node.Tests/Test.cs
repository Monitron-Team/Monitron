using NUnit.Framework;
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Monitron.Node;

namespace Monitron.Node.Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void Serialize()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(NodeConfiguration), new XmlRootAttribute("configuration"));
			FileStream file = new FileStream("TestNode.xml", FileMode.Open);
			NodeConfiguration config = (NodeConfiguration)serializer.Deserialize(file);
		}

		[Test()]
		public void TestNodeConfigSerealizeDesrealize()
		{
			//Serialize
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.NewLineOnAttributes = true;
			XmlWriter xmlWriter = XmlWriter.Create("TestNode.xml", settings);
			NodeConfiguration nodeTestConfig = new NodeConfiguration();
			nodeTestConfig.Account = new Monitron.Common.Account("user", "password", "domain");
			nodeTestConfig.DllPath = "path";
			nodeTestConfig.DllName = "Monitron.Node.Tests";
			nodeTestConfig.Plugin = "Plugin";
			nodeTestConfig.PluginAssemblyName = "Assembly";
			nodeTestConfig.WriteXml(xmlWriter);
			xmlWriter.Close();
			//Deserialize
			NodeConfiguration nodeLoadFromConfig = new NodeConfiguration();
			XmlReader xmlReader = XmlReader.Create("TestNode.xml");
			nodeLoadFromConfig.ReadXml(xmlReader);
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
			NodeConfiguration nodeConfig = new NodeConfiguration();
			XmlReader xmlReader = XmlReader.Create("node.config");
			nodeConfig.ReadXml(xmlReader);
			Node node = new Node(nodeConfig);
			Assert.AreEqual(typeof(Monitron.Node.Tests.TestNodePlugin).ToString(), node.Plugin.ToString());
		}
	}
}

