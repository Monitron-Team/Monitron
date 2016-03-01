using System;
using Monitron.Common;
using System.Xml;

namespace Monitron.Node
{
	public sealed class NodeConfiguration
	{
		public Account Account { get; private set;}
		public string Plugin { get ; private set;}
		public string PluginAssemblyName { get; private set;}
		//TBD - add plugin configuration

		public NodeConfiguration(){
		}
			
		public void LoadConfigFromFile(string i_FilePath)
		{
			XmlDocument doc = getXmlDocument (i_FilePath);
			String userName = doc.GetElementsByTagName ("Username")[0].InnerXml;
			String domain = doc.GetElementsByTagName ("Domain")[0].InnerXml;
			String password = doc.GetElementsByTagName ("Password")[0].InnerXml;
			Plugin = doc.GetElementsByTagName ("Type")[0].InnerXml;
			PluginAssemblyName = doc.GetElementsByTagName ("AssemblyName")[0].InnerXml;
			Account = new Account (userName, password, domain);
		}


		private XmlDocument getXmlDocument(string i_FilePath)
		{
			XmlDocument doc = new XmlDocument ();

			try
			{
				doc.Load (i_FilePath);
			}
			catch (System.IO.FileNotFoundException e)
			{
				throw new Exception("No configuration file found", e);
			}

			return doc;
		}
	}
}

