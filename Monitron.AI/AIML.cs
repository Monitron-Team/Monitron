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
    public class AIML
    {
        private Bot m_bot;
        private readonly Dictionary<string, MethodInfo> r_MethodCache = new Dictionary<string, MethodInfo>();
        private Dictionary<Identity, User> m_Users;
        private readonly IMessengerClient r_MessangerClient;

        public AIML(object i_Object, IMessengerClient i_MessangerClient, XmlDocument i_Doc = null)
        {
            r_MessangerClient = i_MessangerClient;
            r_MessangerClient.MessageArrived += r_MessengerClient_MessageArrived;
            m_bot = new Bot();
            Console.WriteLine(this.m_bot.PathToAIML);
            Console.WriteLine(this.m_bot.PathToConfigFiles);
            m_bot.loadSettings();
            m_Users = new Dictionary<Identity, User>();
            this.initializeMethodsCache(i_Object);
            m_bot.isAcceptingUserInput = false;
            m_bot.loadAIMLFromFiles();
            m_bot.loadAIMLFromXML(i_Doc,"TempName");
            m_bot.isAcceptingUserInput = true;
        }

        public string Request(string i_message, Identity i_Buddy)
        {
            User user = this.getUserFromBuddy(i_Buddy);
            Request r = new Request(i_message, user, this.m_bot);
            Result res = this.m_bot.Chat(r);
            string paramCountStr = user.Predicates.grabSetting("param_count");
            int paramCountInt;
            List<object> parameters =  new List<object>();
            parameters.Add(i_Buddy);
            parameters.Add(new State(user.Predicates));
            if (int.TryParse(paramCountStr, out paramCountInt))
            {
                for (int i = 1; i <= paramCountInt; i++)
                {
                    parameters.Add(user.Predicates.grabSetting("param_" + i.ToString()));
                }
            }
            string input = res.Output;
            Regex rgx = new Regex("{{(\\w+)}}");
            string result = rgx.Replace(input, i_X=> this.r_MethodCache[i_X.Groups[1].Value].Invoke(null, parameters.ToArray()).ToString());
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

        public void UpdateAiml(string i_fullNameSpace, List<Tuple<string, int>> i_methodsToAiml)
        {
            XmlDocument doc = this.CreateXml(i_methodsToAiml);
            this.m_bot.loadAIMLFromXML(doc,i_fullNameSpace+ ".aiml");
        }

        private void initializeMethodsCache(object i_Obj)
        {
            MethodInfo[] myArrayMethodInfo =
                i_Obj.GetType().GetMethods( BindingFlags.Public | 
                                            BindingFlags.Static |
                                            BindingFlags.DeclaredOnly);

            foreach (MethodInfo meth in myArrayMethodInfo)
            {
                var attr = meth.GetCustomAttribute<RemoteCommandAttribute>();
                if (attr != null)
                {
                    r_MethodCache.Add(attr.MethodName, meth);
                }
            }

            //foreach (MethodInfo metod in myArrayMethodInfo)
            //{
            //    this.r_MethodCache.Add(metod.Name, metod);
            //}

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
                returnedValue = this.Request(i_EventArgs.Message, i_EventArgs.Buddy);// ExecuteCommand(i_EventArgs.Buddy, arguments);
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




        private XmlDocument CreateXml( List<Tuple<string, int>> i_methodsToAiml)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);
            XmlNode aimlNode = doc.CreateElement("aiml");
            doc.AppendChild(aimlNode);

            foreach (Tuple<string, int> tuple in i_methodsToAiml)
            {
                XmlNode newcategory = doc.CreateElement("category");
                XmlNode pattern = doc.CreateElement("pattern");
                newcategory.AppendChild(pattern);
                string textPatters = tuple.Item1;
                for (int i = 0; i < tuple.Item2; i++)
                {
                    textPatters += " *";
                }
                doc.CreateTextNode(textPatters);
                pattern.AppendChild(doc.CreateTextNode(textPatters));
                XmlNode think = doc.CreateElement("think");
                XmlNode template = doc.CreateElement("template");
                newcategory.AppendChild(template);
                XmlNode setTag = doc.CreateElement("set");
                XmlAttribute att = doc.CreateAttribute("name");
                att.Value = "method_name" ;
                setTag.Attributes.Append(att);
                setTag.AppendChild(doc.CreateTextNode(tuple.Item1));
                think.AppendChild(setTag);

                XmlNode setTagIsMethod = doc.CreateElement("set");
                XmlAttribute attIsMethod = doc.CreateAttribute("name");
                attIsMethod.Value = "is_method";
                setTagIsMethod.Attributes.Append(attIsMethod);
                setTagIsMethod.AppendChild(doc.CreateTextNode("True"));
                think.AppendChild(setTagIsMethod);

                XmlNode setTagMethodParam = doc.CreateElement("set");
                XmlAttribute attIsMethodParam = doc.CreateAttribute("name");
                attIsMethodParam.Value = "method_param";
                setTagMethodParam.Attributes.Append(attIsMethodParam);

                
                for (int j = 1; j <= tuple.Item2; j++)
                {
                    if (j > 1) //if not first
                        setTagMethodParam.AppendChild(doc.CreateTextNode("_")); //adds space between parameters
                    XmlAttribute starIndex = doc.CreateAttribute("index");
                    starIndex.Value = j.ToString();
                    XmlElement starElement = doc.CreateElement("star");
                    starElement.Attributes.Append(starIndex);
                    setTagMethodParam.AppendChild(starElement);
                }
                 think.AppendChild(setTagMethodParam);


                template.AppendChild(think);
                aimlNode.AppendChild(newcategory);

            }
            return doc;
        }
    }
}
