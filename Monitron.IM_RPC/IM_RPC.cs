using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Monitron.Common;
namespace Monitron.IM_RPC
{




    class Program
    {

       


        static void Main(string[] args)
        {
            Console.WriteLine("Hi!!!");
            testclass testObj = new testclass(3, "asdf");
            Command cmd = Command.Parse("print_details    maorisade");
            string retuenedValue = null;
            bool success = IM_RPC_Cache.ParseExecute(testObj, cmd, out retuenedValue);
            if (success)
            {
                Console.WriteLine("yay!!! Result: " + retuenedValue);
            }
        }


    }

    public class IM_RPC_Cache  //q. is singelton??
    {
        private static Dictionary<Type, Dictionary<string, MethodInfo>> MainCache = new Dictionary<Type, Dictionary<string, MethodInfo>>();
        /// <summary>
        /// perhaps MethodInfo in "Dictionary<string, MethodInfo>>();" should be a list so we can overload
        /// </summary>

        private static Dictionary<string, MethodInfo> GetFromMainCache(Type ObjType)
        {
            Dictionary<string, MethodInfo> Result = null;
            if (MainCache.ContainsKey(ObjType))
            {
                Result = MainCache[ObjType];
            }
            else
            {
                Dictionary<string, MethodInfo> MethodInfoCache = new Dictionary<string, MethodInfo>();
                MethodInfo[] myArrayMethodInfo = ObjType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (MethodInfo meth in myArrayMethodInfo)
                {
                    var att = meth.GetCustomAttribute<RemoteCommandAttribute>(); //maybe try catch
                    if (att != null)
                    {
                        //add to chach
                        if (!MethodInfoCache.ContainsKey(att.MethodName))
                        {
                            MethodInfoCache.Add(att.MethodName, meth);
                        }
                    }
                }
                if (MethodInfoCache.Count > 0)
                {
                    MainCache.Add(ObjType, MethodInfoCache);
                    Result = MethodInfoCache;
                }
            }
            return Result;
        }
        public static bool ParseExecute(object i_Obj, Command i_Command, out string o_ReturnedValue)
        {
            bool result = false;
            object returnValue = null;
            Type ObjType = i_Obj.GetType();
            Dictionary<string, MethodInfo> Methods = GetFromMainCache(ObjType);
            if(Methods.ContainsKey(i_Command.name))
            {
                MethodInfo Method = Methods[i_Command.name];
                ParameterInfo[] PI = Method.GetParameters();
                //var iter = i_Command.Args.GetEnumerator();
                string[] Args = i_Command.Args.ToArray();
                int ParamCount = Args.Length;
                if (ParamCount <= PI.Length) //if too much params
                {
                    object[] ParsedArgs = new object[ParamCount];   //check: will it work for 0 args?)
                    for (int i = 0; i < PI.Length; i++)
                    {
                        //if (PI[i].HasDefaultValue)

                        Type ParamType = PI[i].ParameterType;
                        string currentParam = Args[i];
                        object currInputArg;
                        bool success = TryParse(ParamType, currentParam, out currInputArg);
                        if (success)
                        {
                            ParsedArgs[i] = currInputArg;
                        }
                        else
                        {
                            ///cancle!! or throw , parsing falied
                        }
                        if (i >= ParamCount && i < PI.Length && !(PI[i+1].HasDefaultValue))
                        {
                            /////cancle!! or throw  (missing parameters)
                        }
                    }
                    returnValue =  Method.Invoke(i_Obj, ParsedArgs);
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

        private static bool TryParse(Type i_ParamType, string i_CcurrentParam, out object i_CurrInputArg)
        {
            bool result = false;
            object ParsedObj = null;
            try
            {
                switch (i_ParamType.Name)
                {
                    case "int":
                        {
                            ParsedObj = int.Parse(i_CcurrentParam);
                            break;
                        }
                    case "String":
                        {
                            ParsedObj = i_CcurrentParam;
                            break;
                        }
                    case "double":
                        {
                            ParsedObj = double.Parse(i_CcurrentParam);
                            break;
                        }
                    case "float":
                        {
                            ParsedObj = float.Parse(i_CcurrentParam);
                            break;
                        }
                    default:
                        { //maybe look for function called 'Parse'? or argumentParserAtribute
                            ConstructorInfo ctor = i_ParamType.GetConstructor(new Type[] { i_CcurrentParam.GetType() });
                            ctor.Invoke(new string[] { i_CcurrentParam });
                            break;
                        }
                }
               result = true;
            }
            catch(Exception e)
            {
                //parsing failed!
            }
            i_CurrInputArg = ParsedObj;
            return result;
        }

        private IM_RPC_Cache()  {} //private empty ctor
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteCommandAttribute : Attribute
    {
        public string MethodName;
    }


    public class PluginCommonAdapter
    {
        object m_Obj;
        public PluginCommonAdapter(object i_Obj, IMessengerClient i_MessangerClient)
        {
            m_Obj = i_Obj;
            i_MessangerClient.MessageArrived += DoWhenMessageAvrrived;
        }
        public void DoWhenMessageAvrrived(object sender, MessageArrivedEventArgs e)
        {
            Command cmd = Command.Parse(e.Message);
        }

}
    public class Command
    {
        public string name { get; set; }
        public List<string> Args { get; set; }

        public static Command Parse(string i_Message)
        {
            Command Result = null;
            char[] stringSeparators = new char[] { ' ', '\n', '\t' };
            List<string> Clean = (i_Message.Split(stringSeparators)).ToList<string>();
            if (Clean.Count > 0)
            {
                Result = new Command();
                Result.name = Clean.First();
                Clean.RemoveAt(0);
                Clean.RemoveAll(new Predicate<string>(i => i == ""));

                Result.Args = Clean;
            }
            return Result;

        }
        public Command ()
        {
            Args = new List<string>();
        }
        public VarType GetType(string i_Argument)
        {
            VarType result = VarType.eUnknown;
            int argLength = i_Argument.Length;
            if (argLength >= 2 && (i_Argument.First() == '\"'))
            {
                result = VarType.eString;
            }
            return result;
        }
        public enum VarType
        {
            eUnknown, eInt, eDouble, eFloat, eString, eChar
        }
    }

    public class testclass
    {
        int age;
        string address;

        [RemoteCommand(MethodName = "print_details")]
        public string printdetails(string i_fullName)
        {
            Console.WriteLine("my age is: " + age + "and my address is: " + address + "and my full name is: " + i_fullName);
            return "fucking A!!!!";
        }

        public testclass(int i_age, string i_address)
        {
            age = i_age;
            address = i_address;
        }
    }

}

