using System;
using System.IO;


namespace Monitron.Node
{
	public class Program
	{
		static void Main(string[] args)
		{
			//local installation - the config file will be placed in %appdata%/Monitron folder by the admin
			string appDataFolder = Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData);
			string configFilePath = Path.Combine (appDataFolder, "Monitron", "node.config");
			if (File.Exists(configFilePath))
			{
				NodeConfiguration nodeConfig = new NodeConfiguration();
				nodeConfig.LoadConfigFromFile (configFilePath);
				try
				{
					Node node = new Node (nodeConfig);
				}
				catch (TypeLoadException)
				{
					//couldn't create node due to failing in loading the plugin
				}
			}
		}
	}
}

