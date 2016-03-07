using System;
using Monitron.Common;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Monitron.Node
{
	public sealed class NodeConfiguration : IXmlSerializable
	{
		public Account Account { get; set; }

		public string DllPath { get; set; }
		public string DllName { get; set; }
		public string Plugin { get ; set; }
		public string PluginAssemblyName { get; set; }

		public NodeConfiguration()
        {
		}
			
        public static NodeConfiguration Load(string i_Path)
        {
            using (FileStream file = new FileStream(i_Path, FileMode.Open))
            {
                return Load(file);
            }
        }
        public static NodeConfiguration Load(Stream i_Stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(NodeConfiguration));
            return (NodeConfiguration) serializer.Deserialize(i_Stream);
        }

        public void Save(string i_Path)
        {
            using (FileStream file = new FileStream(i_Path, FileMode.OpenOrCreate))
            {
                Save(file);
            }
        }

        public void Save(Stream i_Stream)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(NodeConfiguration));
            serializer.Serialize(i_Stream, this);
        }
			
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			return(null);
		}

		public void ReadXml(XmlReader reader)
		{
            reader.ReadStartElement("NodeConfiguration");
            string userName = "";
            string domain = "";
            string password = "";

            while (reader.IsStartElement())
            {
                switch (reader.Name)
                {
                    case "Account":
                        reader.ReadStartElement();
                        while (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "Username":
                                    userName = reader.ReadElementContentAsString();
                                    break;
                                case "Domain":
                                    domain = reader.ReadElementContentAsString();
                                    break;
                                case "Password":
                                    password = reader.ReadElementContentAsString();
                                    break;
                            }
                        }

                        Account = new Account(userName, password, domain);
                        break;
                    case "Plugin":
                        reader.ReadStartElement();
                        while (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                        
                                case "DllPath":
                                    DllPath = reader.ReadElementContentAsString();
                                    break;
                                case "DllName":
                                    DllName = reader.ReadElementContentAsString();
                                    break;
                                case "Type":
                                    Plugin = reader.ReadElementContentAsString();
                                    break;
                                case "AssemblyName":
                                    PluginAssemblyName = reader.ReadElementContentAsString();
                                    break;
                            }
                        }
                        break;
                }

                reader.ReadEndElement();
            }
            reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("Account");
			string accountUserName = Account?.Identity.UserName ?? "";
			string accountDomain = Account?.Identity.Domain ?? "";
			string accountPassword = Account?.Password ?? "";
			writer.WriteElementString("Username", accountUserName);
			writer.WriteElementString("Domain", accountDomain);
            writer.WriteElementString("Password", accountPassword);
			//end of Account element
			writer.WriteEndElement();
			writer.WriteStartElement("Plugin");
			writer.WriteElementString("DllPath", DllPath);
			writer.WriteElementString("DllName", DllName);
			writer.WriteElementString("Type", Plugin);
			writer.WriteElementString("AssemblyName", PluginAssemblyName);
			//end of Plugin Element
			writer.WriteEndElement();
		}
	}
}

