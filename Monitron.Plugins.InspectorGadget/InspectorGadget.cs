using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Monitron.AI;
using Monitron.Common;

namespace Monitron.Plugins.InspectorGadget
{
    public class InspectorGadget : INodePlugin
    {
        private readonly IMessengerClient r_Client;
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
           (MethodBase.GetCurrentMethod().DeclaringType);

        public IMessengerClient MessangerClient
        {
            get
            {
                return r_Client;
            }
        }
        private readonly AI.AI r_Ai;

        public InspectorGadget(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
        {
            i_MessangerClient.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            sr_Log.Info("Bumbelbee - Ai integrator starting");
            r_Client = i_MessangerClient;
            sr_Log.Debug("Setting up AI");
            r_Client_ConnectionStateChanged(this, null);
            r_Client = i_MessangerClient;
            this.r_Ai = new AI.AI(this, r_Client);
            using (var fs = new FileStream("CustomizedMethods.aiml", FileMode.Open, FileAccess.Read))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fs);
                this.r_Ai.LoadAIML(doc, "modifications.aiml");
            }
    }


        [RemoteCommand(MethodName = "echo")]
        public static string Echo(Identity i_Buddy, State i_State)
        {
            string input = i_State.getKey("echo::text") ?? "";
            return input;
        }

        [RemoteCommand(MethodName = "add")]
        public static string Add(Identity i_Buddy, State i_State)
        {
            string i_First = i_State.getKey("add::first") ?? "";
            string seconds = i_State.getKey("add::second") ?? "";
            string result = "";
            int first, second;
            if (int.TryParse(i_First, out first) && int.TryParse(seconds, out second))
            {
                result = (first + second).ToString();
            }

            return result;
        }
        [RemoteCommand(MethodName = "movieDetailsByName")]
        public string MovieDetailsByName(Identity i_Buddy, State i_State)
        {
            string name = i_State.getKey("name");
            string query = "http://www.omdbapi.com/?t=" + name + "&y=&plot=short&r=xml";
            string responseFromServer = sendWebRequest(query);
            XDocument doc = XDocument.Parse(responseFromServer);
            string result = "";
            if (doc.Root != null && doc.Root.FirstAttribute.Value == "True")
            {
                XmlReader xmlReader = doc.Root.FirstNode.CreateReader();
                while (xmlReader.Read())
                {
                    string title = xmlReader.GetAttribute("title");
                    string year = xmlReader.GetAttribute("year");
                    xmlReader.GetAttribute("director");
                    string genre = xmlReader.GetAttribute("genre");
                    if (genre != null)
                    {
                        char firstChar = genre.ToLower().First();
                        string aOrAn = (firstChar == 'a' || firstChar == 'o' || firstChar == 'i' || firstChar == 'e')
                                           ? "an"
                                           : "a";
                        result = string.Format("'{0}' is {1} {2} movie from {3}.", title, aOrAn, genre, year);
                    }
                }
            }
            else
            {
                result = "didn't find any movie titles with the name";
            }
            return result;
        }



        [RemoteCommand(MethodName = "imageSearch")]
        public string FindPic(Identity i_Buddy, State i_State)
        {
            MessangerClient.SendMessage(i_Buddy,"Let me search the web...");
            string result;
            string subject = i_State.getKey("imagesearch::subject");
            string query = string.Format(
                "https://pixabay.com/en/photos/?q={0}&image_type=&cat=&min_width=&min_height=",
                subject);
            string responseFromServer = sendWebRequest(query);
            string regexPattern = "(srcset=\")([/0-9a-zA-Z_.\\-]*)";
            Regex rgx = new Regex(regexPattern);
            Match match = rgx.Match(responseFromServer);
            if (match.Success)
            {
                string pictureLink = string.Format("https://pixabay.com/{0}", match.Groups[2].Value);
                result = pictureLink;
            }
            else
            {
                result = string.Format("Couldn't find pictues for {0}", subject); 
            }

            return result;
        }

        
        private string sendWebRequest(string i_Url)
        {
            WebRequest request = WebRequest.Create(i_Url);
            request.Method = "POST";
            string postData = "This is a test that posts this string to a Web server.";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
            Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            Console.WriteLine(responseFromServer);
            reader.Close();
            dataStream.Close();
            response.Close();
            return responseFromServer;
        }

        void r_Client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (r_Client.IsConnected)
            {
                r_Client.SetNickname("Inspector Gadget");
                sr_Log.Debug("Setting up avatar");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.InspectorGadget.inspector_gadget.png"));
                sr_Log.Debug("sending a wellcom message");
                string welcomeMessage = "\nHi, I am Inspector Gadget!";
                    
                foreach (var buddy in r_Client.Buddies)
                {
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
                }
                
            }
        }

    }
}
