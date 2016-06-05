using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

using Monitron.Common;

namespace Monitron.AI.Tests
{
    class TestMethodsClass
    {
        [RemoteCommand(MethodName = "echo")]
        public static string echo(Identity i_Buddy, string i_input)
        {
            return i_input;
        }

        //[RemoteCommand(MethodName = "get_five")]
        public static string getFive(Identity i_Buddy)
        {
            return "5";
        }

        [RemoteCommand(MethodName = "add")]
        public static string add(Identity i_Buddy, string i_First, string i_Seconds)
        {
            string result = "";
            int first, second;
            if (int.TryParse(i_First, out first) && int.TryParse(i_Seconds, out second))
            {
                result = (first + second).ToString();
            }

            return result;
        }
    }
}
