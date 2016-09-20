using AIMLbot;
using Monitron.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Monitron.AI
{
    public class AI
    {
        private Bot m_bot;
        private readonly Dictionary<string, MethodInfo> r_MethodCache = new Dictionary<string, MethodInfo>();
        private Dictionary<Identity, User> m_Users;
        private readonly IMessengerClient r_MessangerClient;
        private readonly object r_Instance;

        public AI(object i_Object, IMessengerClient i_MessangerClient, bool i_LoadDefaults = true)
        {
            r_Instance = i_Object;
            r_MessangerClient = i_MessangerClient;
            r_MessangerClient.MessageArrived += r_MessengerClient_MessageArrived;
            m_bot = new Bot();
            m_bot.loadSettings();
            m_Users = new Dictionary<Identity, User>();
            this.initializeMethodsCache(i_Object);
            m_bot.isAcceptingUserInput = false;
            if (i_LoadDefaults)
            {
                m_bot.loadDefaultAIMLFiles();
            }

            m_bot.isAcceptingUserInput = true;
        }

        public void LoadAIML(XmlDocument i_Doc, string i_Name)
        {
            m_bot.isAcceptingUserInput = false;
            m_bot.loadAIMLFromXML(i_Doc, i_Name);
            m_bot.isAcceptingUserInput = true;
        }

        public string Request(string i_message, Identity i_Buddy)
        {
            User user = this.getUserFromBuddy(i_Buddy);
            Request r = new Request(i_message, user, this.m_bot);
            Result res = this.m_bot.Chat(r);
            List<object> parameters =  new List<object>();
            parameters.Add(i_Buddy);
            parameters.Add(new State(user.Predicates));
            string input = res.Output;
            Regex rgx = new Regex("{{(\\w+)}}");
            string result = rgx.Replace(input, i_X=> this.r_MethodCache[i_X.Groups[1].Value].Invoke(this.r_Instance, parameters.ToArray()).ToString());
            return result;
        }

        private User getUserFromBuddy(Identity i_Buddy)
        {
            User result;
            if (this.m_Users.ContainsKey(i_Buddy))
            {
                result = m_Users[i_Buddy];
            }
            else
            {
                result = new User(i_Buddy.UserName,this.m_bot);  //TODO: generate default user
                m_Users.Add(i_Buddy,result);
            }
            return result;
        }

        private void initializeMethodsCache(object i_Obj)
        {
            MethodInfo[] myArrayMethodInfo = i_Obj.GetType().GetMethods();

            foreach (MethodInfo meth in myArrayMethodInfo)
            {
                var attr = meth.GetCustomAttribute<RemoteCommandAttribute>();
                if (attr != null)
                {
                    r_MethodCache.Add(attr.MethodName, meth);
                }
            }
        }

        public void r_MessengerClient_MessageArrived(object i_Sender, MessageArrivedEventArgs i_EventArgs)
        {
            string messageToParse = "";
            bool isMethod = true;
            string returnedValue = "";
            messageToParse = i_EventArgs.Message;
            string[] arguments = messageToParse.Split(null, 2);
            if (arguments.Length == 0)
            {
                // Nothing to do
                return;
            }
            try
            {
                returnedValue = this.Request(i_EventArgs.Message, i_EventArgs.Buddy);
            }
            catch (TargetInvocationException e)
            {
                AggregateException ag = e.InnerException as AggregateException;
                if (ag != null)
                {
                    returnedValue = string.Join("; ", ag.InnerExceptions.Select(x => x.Message));
                }
                else
                {
                    returnedValue = string.Format(e.InnerException.Message);
                }
            }
            catch (Exception e)
            {
                returnedValue = string.Format("Error executing: \"{0}\": ", i_EventArgs.Message, e.Message);
            }
            try
            {
                r_MessangerClient.SendMessage(i_EventArgs.Buddy, returnedValue);
            }
            catch
            {
                // Server is down. Nothing we can do about it.
            }
        }
    }
}
