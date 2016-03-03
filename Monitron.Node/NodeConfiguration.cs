using System;
using Monitron.Common;
using System.Xml;
using System.Xml.Serialization;

namespace Monitron.Node
{
	public sealed class NodeConfiguration: IXmlSerializable
	{
		public Account Account{ get; set;}
		public string DllPath { get; set; }
		public string DllName{ get; set; }
		public string Plugin{ get ; set;}
		public string PluginAssemblyName{ get; set;}

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
				doc.Load(i_FilePath);
			}
			catch(System.IO.FileNotFoundException e)
			{
				throw new Exception("No configuration file found", e);
			}

			return doc;
		}

		#region IXmlSerializable implementation

		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return(null);
		}

		public void ReadXml(XmlReader reader)
		{
			reader.ReadToFollowing("Username");
			string userName = reader.ReadElementContentAsString();
			reader.ReadToFollowing("Domain");
			string domain = reader.ReadElementContentAsString();
			reader.ReadToFollowing("Password");
			string password = reader.ReadElementContentAsString();
			Account = new Account(userName, password, domain);
			reader.ReadToFollowing("DllPath");
			DllPath = reader.ReadElementContentAsString();
			reader.ReadToFollowing("DllName");
			DllName = reader.ReadElementContentAsString();
			reader.ReadToFollowing("Type");
			Plugin = reader.ReadElementContentAsString();
			reader.ReadToFollowing("AssemblyName");
			PluginAssemblyName = reader.ReadElementContentAsString();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("configuration");
			writer.WriteStartElement("Account");
			writer.WriteStartElement("Identity");
			string accountUserName = Account?.Identity.UserName ?? "";
			string accountDomain = Account?.Identity.Domain ?? "";
			string accountPassword = Account?.Password ?? "";
			writer.WriteElementString("Username", accountUserName);
			writer.WriteElementString("Domain", accountDomain);
			//end of identity element
			writer.WriteEndElement();
			writer.WriteElementString("Password", Account.Password);
			//end of Account element
			writer.WriteEndElement();
			writer.WriteStartElement("Plugin");
			writer.WriteElementString("DllPath", DllPath);
			writer.WriteElementString("DllName", DllName);
			writer.WriteElementString("Type", Plugin);
			writer.WriteElementString("AssemblyName", PluginAssemblyName);
			//end of Plugin Element
			writer.WriteEndElement();
			//end of configuration element
			writer.WriteEndElement();
		}

		#endregion
	}
}

