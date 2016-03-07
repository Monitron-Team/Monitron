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
			string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			string configFilePath = Path.Combine(appDataFolder, k_MainAppDataFolder, k_ConfigName);
            Node node;

			if (File.Exists(configFilePath))
			{
                NodeConfiguration nodeConfig = NodeConfiguration.Load(configFilePath);
				try
				{
					node = new Node(nodeConfig);
				}
				catch(TypeLoadException e)
				{
					//couldn't create node due to failing in loading the plugin
                    Console.WriteLine("Could start node: " + e.Message);
                    return;
				}

                node.Run();
			}

            Console.WriteLine("Could not find configuration");
		}
	}
}

