using System;
using System.Collections.Generic;
using System.Reflection;

namespace Monitron.ImRpc
{
    public class ImRpcCache  //q. needs to be singelton??
    {
        private static Dictionary<Type, Dictionary<string, MethodInfo>> MainCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        private static Dictionary<Type, MethodInfo> ArgumentParsersCache = new Dictionary<Type, MethodInfo>();
        //GetFromMainCache checks if the Type already in cache. in not, reflect the type and add to cache
        //Only for RemoteCommandAttribute
        private static Dictionary<string, MethodInfo> getFromMainCache(Type i_ObjType)
        {
            Dictionary<string, MethodInfo> result = null;
            if (MainCache.ContainsKey(i_ObjType))
            {
                result = MainCache[i_ObjType];
            }
            else
            {
                Dictionary<string, MethodInfo> methodInfoCache = new Dictionary<string, MethodInfo>();
                MethodInfo[] myArrayMethodInfo = i_ObjType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (MethodInfo meth in myArrayMethodInfo)
                {
                    var att = meth.GetCustomAttribute<RemoteCommandAttribute>(); //TODO: maybe try catch
                    if (att != null)
                    {
                        //If the method has the attribute, add to cache
                        if (!methodInfoCache.ContainsKey(att.m_MethodName))
                        {
                            methodInfoCache.Add(att.m_MethodName, meth);
                        }
                    }
                }
                if (methodInfoCache.Count > 0)
                {
                    MainCache.Add(i_ObjType, methodInfoCache);  //add to main cache
                    result = methodInfoCache;
                }
            }
            return result;
        }

        private static MethodInfo getArgumentParserFromCache(Type i_ObjType)
        {
            MethodInfo result = null;
            if (ArgumentParsersCache.ContainsKey(i_ObjType))
            {
                result = ArgumentParsersCache[i_ObjType];
            }
            else
            {
                MethodInfo[] arrayMethodInfo = i_ObjType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                foreach (MethodInfo meth in arrayMethodInfo)
                {
                    var att = meth.GetCustomAttribute<ArgumentParserAttribute>(); //maybe try catch
                    if (att != null)
                    {
                        //add to cache
                        ArgumentParsersCache.Add(i_ObjType, meth);
                        result = meth;
                        break;
                    }
                }
            }
            return result;
        }

        public static bool ParseExecute(object i_Obj, Command i_Command, out string o_ReturnedValue)
        {
            bool result = false;
            object returnValue = null;
            Type objType = i_Obj.GetType();
            Dictionary<string, MethodInfo> methods = getFromMainCache(objType);
            if (methods.ContainsKey(i_Command.Name))
            {
                MethodInfo method = methods[i_Command.Name];
                ParameterInfo[] pi = method.GetParameters();
                //var iter = i_Command.Args.GetEnumerator();
                string[] args = i_Command.Args.ToArray();
                int paramCount = args.Length;
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
                    returnValue = method.Invoke(i_Obj, parsedArgs);
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

        private static bool tryParse(Type i_ParamType, string i_CcurrentParam, out object i_CurrInputArg)
        {
            bool result = false;
            object parsedObj = null;
            try
            {
                //Saggie it didn't accept the Type parameter to the switch :(
                switch (i_ParamType.Name)
                {
                    case "Int32":  // int
                        {
                            parsedObj = int.Parse(i_CcurrentParam);
                            break;
                        }
                    case "String":
                        {
                            parsedObj = i_CcurrentParam;
                            break;
                        }
                    case "Double":
                        {
                            parsedObj = double.Parse(i_CcurrentParam);
                            break;
                        }
                    case "Single":  //float
                        {
                            parsedObj = float.Parse(i_CcurrentParam);
                            break;
                        }
                    default:
                        {
                            MethodInfo argumentParser = getArgumentParserFromCache(i_ParamType);
                            if (argumentParser != null)
                            {
                                parsedObj = argumentParser.Invoke(null, new object[] { i_CcurrentParam });
                            }
                            break;
                        }
                }
                result = true;
            }
            catch (Exception)
            {
                //parsing failed!
            }
            i_CurrInputArg = parsedObj;
            return result;
        }

        private ImRpcCache() { } //private empty ctor
    }

}

