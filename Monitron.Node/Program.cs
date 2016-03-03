using System;
using System.IO;
using System.Xml;

namespace Monitron.Node
{
	public class Program
	{
		private static string k_MainAppDataFolder = "Monitron";
		private static string k_ConfigName = "node.xml";

		static void Main(string[] args)
		{
			//local installation - the config file will be placed in %appdata%/Monitron folder by the admin
			string appDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			string configFilePath = Path.Combine(appDataFolder, k_MainAppDataFolder, k_ConfigName);

			if (File.Exists(configFilePath))
			{
				NodeConfiguration nodeConfig = new NodeConfiguration();
				XmlReader xmlReader = XmlReader.Create(configFilePath);
				nodeConfig.ReadXml(xmlReader);
				try
				{
					Node node = new Node(nodeConfig);
				}
				catch(TypeLoadException)
				{
					//couldn't create node due to failing in loading the plugin
				}
			}
		}
	}
}

