using System.Threading;
using NUnit.Framework;
using Monitron.Clients.Mock;
using Monitron.Common;

namespace Monitron.ImRpc.Tests
{    public class ImRpcTests
    {
        [Test()]
        public void TestParantesis()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            new RpcAdapter(new TestClass(), client);
            string expectedResponse = "Hello World";
            client.PushMessage(friendIdentity, string.Format("echo  \"{0}\"", expectedResponse));
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);
            
        }

        [Test()]
        public void TestCustomConversion()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            new RpcAdapter(new TestClass(), client);
            string expectedResult = "Hello World;-13";
            client.PushMessage(friendIdentity, string.Format("convert_echo  \"{0}\"", expectedResult));
            Assert.AreEqual(expectedResult, client.SentMessageQueue.Dequeue().Item2);
        }

        [Test()]
        public void TestComplexCommand()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            new RpcAdapter(new TestClass(), client);
            string expectedResult = "word 3 1.5 \"multiple words\"";
            client.PushMessage(friendIdentity, string.Format("complex_cmd  {0}", expectedResult));
            Assert.AreEqual(expectedResult, client.SentMessageQueue.Dequeue().Item2);
        }
        
    }
}