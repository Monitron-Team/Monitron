using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using NDesk.Options;

namespace Monitron.Node
{
	public class Program
	{
		private static string k_MainAppDataFolder = "Monitron";
		private static string k_ConfigName = "node.config";
		private static string k_ConfigParam = "conf=";
		private static string k_HelpParam = "help";

		private static void showHelpMessage(OptionSet i_Options)
		{
			Console.WriteLine("Usage:");
			i_Options.WriteOptionDescriptions(Console.Out);
		}

		static void Main(string[] args)
		{
			string configFilePath = null;

			//getting installation params from user
			if(args.Length >= 1) 
			{
				bool showHelp = false;
				var options = new OptionSet() {
					{ k_ConfigParam, "full path of the node configuration", value => configFilePath = value},
					{ k_HelpParam, "show help message", value => showHelp = value != null}
				};

				List<string> paramList;
				try
				{
					paramList = options.Parse(args);
				}
				catch (OptionException e) 
				{
					Console.WriteLine(e.Message);
					Console.WriteLine("try use --help for more information");
					return;
				}

				//print the help message
				if (showHelp) 
				{
					showHelpMessage(options);
					return;
				}
			}
			//local installation - the config file will be placed in %appdata%/Monitron folder by the admin
			else
			{
				string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
				configFilePath = Path.Combine(appDataFolder, k_MainAppDataFolder, k_ConfigName);
			}

            Node node;
			NodeConfiguration nodeConfig;

			if (File.Exists(configFilePath))
			{
				try
				{
					nodeConfig = NodeConfiguration.Load(configFilePath);
				}
				catch(InvalidOperationException e) 
				{
					Console.WriteLine("Could not load node configuration: " + e.Message);
					return;
				}
				try
				{
					node = new Node(nodeConfig);
				}
				catch(TypeLoadException e)
				{
					//couldn't create node due to failing in loading the plugin
                    Console.WriteLine("Could not start node: " + e.Message);
                    return;
				}
		
                node.Run();
			}

            Console.WriteLine("Could not find configuration");
		}
	}
}

