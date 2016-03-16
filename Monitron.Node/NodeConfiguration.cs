using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using Monitron.Common;
using Monitron.PluginDataStore.Local;

namespace Monitron.Node
{
	public sealed class NodeConfiguration : IXmlSerializable
	{
		public Account Account { get; set; }
		public NodePlugin Plugin { get; set; }
		public MessangerClient MessageClient { get; set; }
		public IPluginDataStore DataStore { get; set; }

		public class NodePlugin
		{
			internal NodePlugin(){}

			public string DllPath { get; set; }
			public string DllName { get; set; }
			public string Type { get; set; }
		}

		public class MessangerClient
		{
			internal MessangerClient(){}

			public string DllPath { get; set; }
			public string DllName { get; set; }
			public string Type { get; set; }
		}
			
		public NodeConfiguration()
        {
			Plugin = new NodePlugin();
			MessageClient = new MessangerClient();
		}
			
        public static NodeConfiguration Load(string i_Path)
        {
            using (FileStream file = new FileStream(i_Path, FileMode.Open, FileAccess.Read))
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
				case "MessangerClient":
					reader.ReadStartElement();
					while (reader.IsStartElement())
					{
						switch (reader.Name)
						{
						case "DllPath":
							MessageClient.DllPath = reader.ReadElementContentAsString();
							break;
						case "DllName":
							MessageClient.DllName = reader.ReadElementContentAsString();
							break;
						case "Type":
							MessageClient.Type = reader.ReadElementContentAsString();
							break;
						}
					}
					break;
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
								Plugin.DllPath = reader.ReadElementContentAsString();
                                break;
                            case "DllName":
								Plugin.DllName = reader.ReadElementContentAsString();
                                break;
                            case "Type":
								Plugin.Type = reader.ReadElementContentAsString();
                                break;
                        }
                    }
                    break;
				case "DataStore":
					string location = string.Empty;
					reader.ReadStartElement();
					while(reader.IsStartElement()) 
					{
						switch(reader.Name) 
						{
						case "Location":
							location = reader.ReadElementContentAsString();
							break;
						}
					}
					if(File.Exists(location)) 
					{
						DataStore = new LocalPluginDataStore(location);
					}
					break;
            	}

                reader.ReadEndElement();
            }
            reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			writer.WriteStartElement("MessangerClient");
			writer.WriteElementString("DllPath", MessageClient.DllPath);
			writer.WriteElementString("DllName", MessageClient.DllName);
			writer.WriteElementString("Type", MessageClient.Type);
			writer.WriteEndElement();
			//end of MessangerClient element
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
			writer.WriteElementString("DllPath", Plugin.DllPath);
			writer.WriteElementString("DllName", Plugin.DllName);
			writer.WriteElementString("Type", Plugin.Type);
			//end of Plugin Element
			writer.WriteEndElement();
			writer.WriteStartElement("DataStore");
			writer.WriteElementString("Location", "TBD");
			//end of DataStore Element
			writer.WriteEndElement();
		}
	}
}

