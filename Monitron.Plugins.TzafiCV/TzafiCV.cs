using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using Monitron.AI;
using Monitron.Common;

namespace Monitron.Plugins.TzafiCV
{
    class TzafiCV: INodePlugin
    {
        private readonly IMessengerClient r_Client;
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
           (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IMessengerClient MessangerClient
        {
            get
            {
                return r_Client;
            }
        }
        private readonly AI.AI r_Ai;

        public TzafiCV(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
        {
            i_MessangerClient.ConnectionStateChanged += r_Client_ConnectionStateChanged;
            sr_Log.Info("TzafiCV");
            r_Client = i_MessangerClient;
            sr_Log.Debug("Setting up AI");
            r_Client_ConnectionStateChanged(this, null);
            r_Client = i_MessangerClient;
            this.r_Ai = new AI.AI(this, r_Client,false);
            using (var fs = new FileStream("./TzafiCV.aiml", FileMode.Open, FileAccess.Read))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fs);
                this.r_Ai.LoadAIML(doc, "TzafiCV.aiml");
            }
        }

        void r_Client_ConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            if (r_Client.IsConnected)
            {
                r_Client.SetNickname("Tzafi's CV");
                sr_Log.Debug("Setting up avatar");
                r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.TzafiCV.cv_avatar.png"));
                sr_Log.Debug("sending a wellcom message");
                string welcomeMessage = "\nHi, I am Tzafi's CV!";

                foreach (var buddy in r_Client.Buddies)
                {
                    r_Client.SendMessage(buddy.Identity, welcomeMessage);
                }

            }
        }

    }
}
