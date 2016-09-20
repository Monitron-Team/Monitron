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
        private XmlDocument testXml()
        {
            using (var fs = new FileStream("./TestXml.xml", FileMode.Open, FileAccess.Read))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fs);
                return doc;
            }
        }

        [Test()]
        public void TestSimpleEcho()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            string wordToRepeat = "Bamba";
            string expectedResponse = "You said: " + wordToRepeat + ".";
            TestMethodsClass testClass = new TestMethodsClass();
            AI bot = new AI(testClass, client);
            bot.LoadAIML(testXml(), "test");
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
            TestMethodsClass testClass = new TestMethodsClass();
            AI bot = new AI(testClass, client);
            bot.LoadAIML(testXml(), "test");
            client.PushMessage(friendIdentity, string.Format("add {0} {1}", first, second));
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);

        }

        [Test()]
        public void TestMovies()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            TestMethodsClass testClass = new TestMethodsClass();
            AI bot = new AI(testClass, client);
            bot.LoadAIML(testXml(), "test");
            string res = bot.Request("my name is daniel", friendIdentity);
            //res = bot.Request("I am 27 years old", friendIdentity);
            //res = bot.Request("what is my name?", friendIdentity);
            client.PushMessage(friendIdentity, "give me a movie title with my name");
            Assert.AreEqual("false", client.SentMessageQueue.Dequeue().Item2);
            //Assert.AreEqual(expectedResponse, res);

        }

        [Test()]
        public void pic()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            TestMethodsClass testClass = new TestMethodsClass();
            AI bot = new AI(testClass, client);
            bot.LoadAIML(testXml(), "test");
            client.PushMessage(friendIdentity, "show me a picture of babies");
            Assert.AreEqual("false", client.SentMessageQueue.Dequeue().Item2);
            //TestMethodsClass.findPic("tree");

        }
    }
}