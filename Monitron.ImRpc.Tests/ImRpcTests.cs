using System.Threading;

using NUnit.Framework;

using Monitron.Clients.Mock;
using Monitron.Common;

namespace Monitron.ImRpc.Tests
{    public class ImRpcTests
    {
        [Test()]
        public void TestParsingOnMessageArrive()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity() { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            TestClass demoPlugInObj = new TestClass(3, "SampleText");
            new RpcAdapter(demoPlugInObj, client);
            client.PushMessage(friendIdentity, "print_with_Args_Parser    texty 22  4.234 113.5  aaa_TextToBePasedByArgumentParser_aaa");
            string ExpectedResponse =
                "Parsed arg number: 99 parsed arg text: 999_TextToBeP9sedByArgumentP9rser_999 string: textyint: 22 float: 4.234 double: 113.5";
            Thread.Sleep(500);
            Assert.AreEqual(client.SentMessageQueue.Dequeue().Item2, ExpectedResponse);
        }

        [Test()]
        public void GenericTestParsingOnMessageArrive(object i_Plugin, string i_Message, string i_Expected)
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            new RpcAdapter(i_Plugin, client);
            client.PushMessage(friendIdentity, i_Message);
            Thread.Sleep(500);
            Assert.AreEqual(client.SentMessageQueue.Dequeue().Item2, i_Expected);
        }
    }
}