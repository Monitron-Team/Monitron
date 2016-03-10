//using NUnit.Framework;

using System;

using Monitron.Common;

namespace Monitron.ImRpc.Tests
{

    public class ImRpcTests
    {
        static void test()
        {
            Testclass testObj = new Testclass(3, "SampleText");
            string retuenedValue = null;
            IMessengerClient MesssengerClient = (IMessengerClient)new object();
            Command cmd = Command.Parse("print_with_Args_Parser    texty 22  4.234 113.5  aaa_TextToBePasedByArgumentParser_aaa");
            PluginCommonAdapter commonAdapter = new PluginCommonAdapter(testObj, MesssengerClient);
            bool success = commonAdapter.ParseExecute(cmd, out retuenedValue);
            if (success)
            {
                Console.WriteLine(retuenedValue);
            }
        }
    }
}
