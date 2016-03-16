using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using Mono.Options;
using log4net.Repository.Hierarchy;
using log4net;
using log4net.Layout;
using log4net.Appender;
using log4net.Core;

namespace Monitron.Node
{
	public class Program
	{
        private static readonly ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
		private static string k_MainAppDataFolder = "Monitron";
		private static string k_ConfigName = "node.config";
		private static string k_ConfigParam = "conf=";
		private static string k_HelpParam = "help";

		private static void showHelpMessage(OptionSet i_Options)
		{
			Console.WriteLine("Usage:");
			i_Options.WriteOptionDescriptions(Console.Out);
		}

        private static void setUpLogging()
        {
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();

            PatternLayout patternLayout = new PatternLayout();
            patternLayout.ConversionPattern = "[%thread] %-5level %logger - %message%newline";
            patternLayout.ActivateOptions();

            ConsoleAppender appender = new ConsoleAppender();
            appender.Layout = patternLayout;
            appender.ActivateOptions();
            hierarchy.Root.AddAppender(appender);

            hierarchy.Root.Level = Level.Debug;
            hierarchy.Configured = true;
        }

		static void Main(string[] args)
		{
            setUpLogging();
			string configFilePath = null;

			// Getting installation params from user
			if(args.Length >= 1) 
			{
				bool showHelp = false;
				var options = new OptionSet() {
					{ k_ConfigParam, "full path of the node configuration", value => configFilePath = value},
					{ k_HelpParam, "show help message", value => showHelp = value != null}
				};

				try
				{
					options.Parse(args);
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

            sr_Log.Info("Starting Node");
            runNode(configFilePath);
		}

        static void runNode(string configFilePath)
        {
            Node node;
            NodeConfiguration nodeConfig;
            if (File.Exists(configFilePath))
            {
                try
                {
                    sr_Log.Info("Reading configuration");
                    nodeConfig = NodeConfiguration.Load(configFilePath);
                }
                catch (InvalidOperationException e)
                {
                    sr_Log.ErrorFormat("Could not load node configuration: {0}", e.Message);
                    return;
                }

                try
                {
                    node = new Node(nodeConfig);
                }
				catch (Exception e)
                {
                    //couldn't create node due to failing in loading the plugin
                    sr_Log.ErrorFormat("Could not start node: {0}", e.Message);
                    return;
                }

                node.Run();
            }
            else
            {
                sr_Log.ErrorFormat("Could not find configuration");
            }
        }
	}
}

