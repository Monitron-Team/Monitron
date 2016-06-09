using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml;
using Monitron.Clients.Mock;
using Monitron.Common;
using NUnit.Framework;
namespace Monitron.AI.Tests
{
    public class AiTests
    {
        [Test()]
        public void TestSimpleEcho()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            string wordToRepeat = "Bamba";
            string expectedResponse = "You said: " + wordToRepeat + ".";
            TestMethodsClass testClass = new TestMethodsClass(client);
            client.PushMessage(friendIdentity, "echo " + wordToRepeat);
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);
        }

        [Test()]
        public void TestAdd()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            int first = 4;
            int second = 5;
            string expectedResponse = string.Format("{0} + {1} = {2}.", first, second, first + second);
            TestMethodsClass testClass = new TestMethodsClass(client);
            client.PushMessage(friendIdentity, string.Format("add {0} {1}", first, second));
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);
        }

        [Test()]
        public void TestMovies()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            TestMethodsClass testClass = new TestMethodsClass(client);
            client.PushMessage(friendIdentity, "give me a movie title with my name");
            Assert.AreEqual("this_will_always_fail", client.SentMessageQueue.Dequeue().Item2); 
                //todo: regex for respons. this will always fail because the respones depends on the name 

        }
        [Test()]
        public void TestTzafiCV()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            string expectedResponse = "Mifratz Shlomo 27, Holon";
            TestMethodsClass testClass = new TestMethodsClass(client);
            client.PushMessage(friendIdentity, "Where do you live");
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);
        }
       


    }
}