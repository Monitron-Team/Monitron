using System;
using System.Collections.Generic;
using System.Reflection;

using Monitron.Common;


namespace Monitron.ImRpc
{
    public class RpcAdapter
    {
        object m_Obj;
        IMessengerClient m_MessangerClient;
        private readonly Dictionary<string, MethodInfo> r_MainCache = new Dictionary<string, MethodInfo>();
        private readonly Dictionary<Type, MethodInfo> r_ArgumentParsersCache = new Dictionary<Type, MethodInfo>();

        public RpcAdapter(object i_Obj, IMessengerClient i_MessangerClient)
        {
            m_Obj = i_Obj;
            this.r_MainCache = new Dictionary<string, MethodInfo>();
            this.r_ArgumentParsersCache = new Dictionary<Type, MethodInfo>();
            this.addMethodsToMainCache();
            addArgumentParsersToCache();
            i_MessangerClient.MessageArrived += DoWhenMessageAvrrived;
            m_MessangerClient = i_MessangerClient;
        }

        public void DoWhenMessageAvrrived(object i_Sender, MessageArrivedEventArgs i_EventArgs)
        {
            Command cmd = Command.Parse(i_EventArgs.Message);
            string retuenedValue;
            bool wasSuccess = ParseExecute(cmd, out retuenedValue);
            if (wasSuccess)
            {
                this.m_MessangerClient.SendMessage(i_EventArgs.Buddy, retuenedValue);
            }
            else
            {
                retuenedValue = string.Format("Error executing: \"{0}\"", i_EventArgs.Message);
            }
            try
            {
                m_MessangerClient.SendMessage(i_EventArgs.Buddy, retuenedValue);
            }
            catch (Exception)
            {
                //todo
            }
        }

        private void addMethodsToMainCache()
        {
            MethodInfo[] myArrayMethodInfo =
             m_Obj.GetType().GetMethods(BindingFlags.Public
                                      | BindingFlags.Instance
                                      | BindingFlags.DeclaredOnly);
            foreach (MethodInfo meth in myArrayMethodInfo)
            {
                var attr = meth.GetCustomAttribute<RemoteCommandAttribute>(); //TODO: maybe try catch
                if (attr != null)
                {
                    //If the method has the attribute, add to cache
                    if (!this.r_MainCache.ContainsKey(attr.MethodName))
                    {
                        this.r_MainCache.Add(attr.MethodName, meth);
                    }
                }
            }
        }

        private void addArgumentParsersToCache()
        {
            MethodInfo[] arrayMethodInfo = m_Obj.GetType().GetMethods(BindingFlags.Public
                                                                    | BindingFlags.Static
                                                                    | BindingFlags.DeclaredOnly);
            foreach (MethodInfo meth in arrayMethodInfo)
            {
                var att = meth.GetCustomAttribute<ArgumentParserAttribute>(); //maybe try catch
                if (att != null)
                {
                    //add to cache
                    this.r_ArgumentParsersCache.Add(meth.ReturnType, meth);
                }
            }
        }

        internal bool ParseExecute(Command i_Command, out string o_ReturnedValue)
        {
            bool result = false;
            object returnValue = null;
            if (this.r_MainCache.ContainsKey(i_Command.Name))
            {
                MethodInfo method = this.r_MainCache[i_Command.Name];
                ParameterInfo[] pi = method.GetParameters();
                //var iter = i_Command.Args.GetEnumerator();
                IList<string> args = i_Command.Args;//.ToArray();
                int paramCount = args.Count;
                if (paramCount <= pi.Length) //if too much params
                {
                    object[] parsedArgs = new object[paramCount];   //check: will it work for 0 args?)
                    for (int i = 0; i < pi.Length; i++)
                    {
                        Type paramType = pi[i].ParameterType;
                        string currentParam = args[i];
                        object currInputArg;
                        bool success = tryParse(paramType, currentParam, out currInputArg);
                        if (success)
                        {
                            parsedArgs[i] = currInputArg;
                        }
                        else
                        {
                            //cancle!! or throw , parsing falied
                        }
                        if (i >= paramCount && i < pi.Length && !(pi[i + 1].HasDefaultValue))
                        {
                            /////cancle!! or throw  (missing parameters)
                        }
                    }

                    returnValue = method.Invoke(this.m_Obj, parsedArgs);
                    result = true;
                }
            }
            if (returnValue != null)
            {
                o_ReturnedValue = returnValue.ToString();
            }
            else
            {
                o_ReturnedValue = null;
            }
            return result;
        }

        private bool tryParse(Type i_ParamType, string i_CcurrentParam, out object i_CurrInputArg)
        {
            bool result = false;
            object parsedObj = null;
            try
            {
                if (this.r_ArgumentParsersCache.ContainsKey(i_ParamType))
                {
                    MethodInfo argumentParser = this.r_ArgumentParsersCache[i_ParamType];
                    parsedObj = argumentParser.Invoke(null, new object[] { i_CcurrentParam });
                }
                else
                {
                    parsedObj = Convert.ChangeType(i_CcurrentParam, i_ParamType);
                }
            }
            catch (Exception)
            {
                //parsing failed!
            }

            if (parsedObj != null)
            {
                result = true;
            }

            i_CurrInputArg = parsedObj;

            return result;
        }

        public static T ChangeType<T>(object i_Obj)
        {
            return (T)Convert.ChangeType(i_Obj, typeof(T));
        }
    }
}
