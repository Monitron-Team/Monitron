using System;
using System.Linq;
using System.Reflection;
using Mono.Options;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Monitron.Common;

namespace Monitron.ImRpc
{
    internal class RpcMethod
    {
        private readonly OptionSet r_OptionSet;
        private readonly ParameterInfo[] r_ParameterInfos;
        private readonly Func<object, Identity, string[], string> r_func;
        private readonly object r_Instance;
        private readonly string r_HelpString;
        private readonly string r_Name;
        private readonly string r_Description;
        private readonly string r_ShortDescription;

        public string Name
        {
            get
            {
                return r_Name;
            }
        }

        public string Description
        {
            get
            {
                return r_Description;
            }
        }

        public string ShortDescription
        {
            get
            {
                return r_ShortDescription;
            }
        }

        public RpcMethod(string name, string description, MethodInfo i_MethodInfo, object i_Instance)
        {
            r_Name = name;
            r_Description = description;
            if (description != null)
            {
                r_ShortDescription = description.Split(new [] { '\n' }, 2)[0].Trim();
            }

            r_OptionSet = new OptionSet();
            r_ParameterInfos = i_MethodInfo.GetParameters();
            r_func = fromFunc(i_MethodInfo);
            r_HelpString = getHelpString();
            r_Instance = i_Instance;
        }

        private static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            char quotesChar = '\0';
            return commandLine.Split(c =>
                {
                    if (c == '\'' || c == '\"')
                    {
                        if (quotesChar == '\0')
                        {
                            quotesChar = c;
                        }
                        else if (quotesChar == c)
                        {
                            quotesChar = '\0';
                        }
                    }

                    return quotesChar == '\0' && char.IsWhiteSpace(c);
                })
                    .Select(arg => arg.Trim().TrimMatchingQuotes())
                    .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public string Invoke(Identity i_Buddy, string i_Parameters)
        {
            return r_func(r_Instance, i_Buddy, SplitCommandLine(i_Parameters).ToArray());
        }

        public string Invoke(Identity i_Buddy, params string[] i_Parameters)
        {
            return r_func(r_Instance, i_Buddy, i_Parameters);
        }

        private string getHelpString()
        {
            StringWriter sw = new StringWriter();
            sw.Write("Usage: ");
            sw.Write(r_Name);
            sw.Write(" ");
            foreach (ParameterInfo pi in r_ParameterInfos.Skip(1))
            {
                OptAttribute opt = (OptAttribute) pi.GetCustomAttribute(typeof(OptAttribute));
                sw.Write("<");
                if (opt != null)
                {
                    sw.Write(opt.Name);
                }
                else
                {
                    sw.Write(pi.Name);
                }

                sw.Write("> ");
            }

            if (r_Description != null)
            {
                sw.Write("\n");
                sw.Write(r_Description.Trim());
            }

            if (r_OptionSet.Count > 0)
            {
                sw.Write("\n");
                sw.Write("Options:\n");
                r_OptionSet.WriteOptionDescriptions(sw);
            }

            return sw.ToString().TrimEnd();
        }

        public string HelpString {
            get
            {
                return r_HelpString;
            }
        }

        private object convertTo(Type i_ParameterType, string i_Value)
        {
            if (i_ParameterType == typeof(string))
            {
                return i_Value;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(i_ParameterType);
            if (converter != null && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFrom(i_Value);
            }

            return Convert.ChangeType(i_Value, i_ParameterType);
        }

        private Func<object, Identity, string[], string> fromFunc(MethodInfo i_MethodInfo)
        {
            object[] parameters = new object[r_ParameterInfos.Length];
            bool skipFirst = false;
            foreach (var pi in r_ParameterInfos)
            {
                if (!skipFirst)
                {
                    // First is the buddy arg
                    skipFirst = true;
                    continue;
                }

                OptAttribute opt = (OptAttribute) pi.GetCustomAttribute(typeof(OptAttribute));
                if (opt == null)
                {
                    continue;
                }

                r_OptionSet.Add(
                    prototype: opt.Prototype,
                    description: opt.Description,
                    action: v => parameters[pi.Position] = convertTo(pi.ParameterType, v)
                );
            }

            return delegate (object i_Instance, Identity i_Buddy, string[] i_Arguments)
            {
                Array.Clear(parameters, 0, parameters.Length);
                List<string> extra = r_OptionSet.Parse(i_Arguments);
                parameters[0] = i_Buddy;
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (parameters[i] == null)
                    {
                        if (i <= extra.Count)
                        {
                            parameters[i] = this.convertTo(r_ParameterInfos[i].ParameterType, extra[i - 1]);
                        }
                        else
                        {
                            parameters[i] = r_ParameterInfos[i].DefaultValue;
                        }
                    }
                }

                return (string)i_MethodInfo.Invoke(i_Instance, parameters);
            };
        }
    }
}

