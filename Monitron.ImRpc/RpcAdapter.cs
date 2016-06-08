using System;
using System.Collections.Generic;
using Monitron.Common;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.IO;

namespace Monitron.ImRpc
{
    public class RpcAdapter
    {
        private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private readonly object r_Obj;
        private readonly IMessengerClient r_MessangerClient;
        private readonly Dictionary<string, RpcMethod> r_MethodCache = new Dictionary<string, RpcMethod>();

        public RpcAdapter(object i_Obj, IMessengerClient i_MessangerClient)
        {
            r_Obj = i_Obj;
            this.initializeMethodsCache();
            r_MessangerClient = i_MessangerClient;
            r_MessangerClient.MessageArrived += r_MessengerClient_MessageArrived;
        }

        public void r_MessengerClient_MessageArrived(object i_Sender, MessageArrivedEventArgs i_EventArgs)
        {
            string[] arguments = i_EventArgs.Message.Split(null, 2);
            if (arguments.Length == 0)
            {
                // Nothing to do
                return;
            }
                
            string returnedValue;
            try
            {
                returnedValue = ExecuteCommand(i_EventArgs.Buddy, arguments);
            }
            catch(TargetInvocationException e)
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

                sr_Log.Error("Error during rpc command", e);
            }
            catch (Exception e)
            {
                returnedValue = string.Format("Error executing: \"{0}\": {1}", i_EventArgs.Message, e.Message);
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

        private void initializeMethodsCache()
        {
            MethodInfo[] myArrayMethodInfo =
             r_Obj.GetType().GetMethods(BindingFlags.Public
                                      | BindingFlags.Instance
                                      | BindingFlags.DeclaredOnly);
            r_MethodCache.Add(
                "help",
                new RpcMethod(
                    "help",
                    "Prints help",
                    this.GetType().GetMethod("help", BindingFlags.NonPublic | BindingFlags.Instance),
                    this)
            );
            foreach (MethodInfo meth in myArrayMethodInfo)
            {
                var attr = meth.GetCustomAttribute<RemoteCommandAttribute>();
                if (attr != null)
                {
                    r_MethodCache.Add(
                        attr.MethodName,
                        new RpcMethod(attr.MethodName, attr.Description, meth, r_Obj)
                    );
                }
            }
        }

        private string printMethodList()
        {
            StringWriter sw = new StringWriter();
            sw.Write("Available commands:\n");
            foreach (var rpcMethod in r_MethodCache.Values)
            {
                sw.Write(rpcMethod.Name);
                if (rpcMethod.ShortDescription != null)
                {
                    sw.Write("\n\t");
                    sw.Write(rpcMethod.ShortDescription);
                }
                sw.Write("\n");
            }
            return sw.ToString();
        }

        private string help(
            Identity i_Buddy,
            [Opt("command_name", "c|command=", "Gets help about {COMMAND}")] string i_CommandName = null
        )
        {
            if (i_CommandName != null)
            {
                RpcMethod method;

                if (!r_MethodCache.TryGetValue(i_CommandName, out method))
                {
                    string.Format("Command '{0}' does not exist", i_CommandName);
                }
                else
                {
                    return method.HelpString;
                }
            }

            return printMethodList();
        }

        internal string ExecuteCommand(Identity i_Buddy, string[] i_Arguments)
        {
            string commandName = i_Arguments[0];
            RpcMethod method;
            if (!this.r_MethodCache.TryGetValue(commandName, out method))
            {
                throw new KeyNotFoundException(string.Format("Command '{0}' does not exist", commandName));
            }
            if (i_Arguments.Length == 1)
            {
                return method.Invoke(i_Buddy);
            }
            else
            {
                return method.Invoke(i_Buddy, i_Arguments[1]);
            }
        }
    }
}
