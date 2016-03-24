using System;
using System.Collections.Generic;
using Monitron.Common;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;

using System.ComponentModel;

namespace Monitron.ImRpc
{
    public class RpcAdapter
    {
        private readonly object r_Obj;
        private readonly IMessengerClient r_MessangerClient;
        private readonly Dictionary<string, MethodInfo> r_MethodCache = new Dictionary<string, MethodInfo>();
        private static readonly Regex sr_SplitRegex = new Regex(@"\""([^}]+?(?<!\\))\""|\S+");

        public RpcAdapter(object i_Obj, IMessengerClient i_MessangerClient)
        {
            r_Obj = i_Obj;
            this.r_MethodCache = new Dictionary<string, MethodInfo>();
            this.initializeMethodsCache();
            r_MessangerClient = i_MessangerClient;
            r_MessangerClient.MessageArrived += r_MessengerClient_MessageArrived;
        }

        private string[] parseCommand(string i_RawCommand)
        {
            return sr_SplitRegex.Matches(i_RawCommand)
                .Cast<Match>()
                .Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[0].Value)
                .ToArray();
        }

        public void r_MessengerClient_MessageArrived(object i_Sender, MessageArrivedEventArgs i_EventArgs)
        {
            string[] arguments = parseCommand(i_EventArgs.Message);
            if (arguments.Length == 0)
            {
                // Nothing to do
                return;
            }
                
            string retuenedValue;
            try
            {
                retuenedValue = ExecuteCommand(arguments);
            }
            catch(TargetInvocationException e)
            {
                retuenedValue = string.Format(e.InnerException.Message);
            }
            catch (Exception e)
            {
                retuenedValue = string.Format("Error executing: \"{0}\": ", i_EventArgs.Message, e.Message);
            }

            try
            {
                r_MessangerClient.SendMessage(i_EventArgs.Buddy, retuenedValue);
            }
            catch
            {
                // Server is down. Nothing we can do about it.
            }
        }

        private void initializeMethodsCache()
        {
            MethodInfo[] myArrayMethodInfo =
             r_Obj.GetType().GetMethods(BindingFlags.Public
                                      | BindingFlags.Instance
                                      | BindingFlags.DeclaredOnly);
            foreach (MethodInfo meth in myArrayMethodInfo)
            {
                var attr = meth.GetCustomAttribute<RemoteCommandAttribute>();
                if (attr != null)
                {
                    //If the method has the attribute, add to cache
                    if (!this.r_MethodCache.ContainsKey(attr.MethodName))
                    {
                        this.r_MethodCache.Add(attr.MethodName, meth);
                    }
                }
            }
        }
            
        internal string ExecuteCommand(string[] i_Arguments)
        {
            string commandName = i_Arguments[0];
            if (!this.r_MethodCache.ContainsKey(commandName))
            {
                throw new KeyNotFoundException(string.Format("Command '{0}' does not exist", commandName));
            }

            MethodInfo method = this.r_MethodCache[commandName];
            ParameterInfo[] paremeterInfos = method.GetParameters();
            object[] parsedArgs = new object[paremeterInfos.Length];
            int index = 1;
            foreach (ParameterInfo parameterInfo in paremeterInfos)
            {
                if (i_Arguments.Length == index)
                {
                    break;
                }

                object currentInputArgument;
                string argument = i_Arguments[index];
                if (!tryParse(parameterInfo.ParameterType, argument, out currentInputArgument))
                {
                    throw new FormatException(
                        string.Format(
                            "Value {0} is not valid for parameter {1}",
                            argument[0],
                            parameterInfo.Name
                        )
                    );
                }

                parsedArgs[index - 1] = currentInputArgument;
                index++;
            }
                    
            object returnValue = method.Invoke(this.r_Obj, parsedArgs);
            if (returnValue != null)
            {
                return returnValue.ToString();
            }
            else
            {
                return null;
            }
        }

        private bool tryParse(Type i_ParamType, string i_CurrentParam, out object i_CurrInputArg)
        {
            bool result = false;
            object parsedObj = null;
            TypeConverter converter = TypeDescriptor.GetConverter(i_ParamType);
               
            try
            {
                if (converter.CanConvertFrom(typeof(string)))
                {
                    parsedObj = converter.ConvertFrom(i_CurrentParam);
                }
                else
                {
                    parsedObj = Convert.ChangeType(i_CurrentParam, i_ParamType);
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
    }
}
