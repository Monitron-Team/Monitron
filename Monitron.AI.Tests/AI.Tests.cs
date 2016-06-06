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
    public class ImRpcTests
    {
       

        [Test()]
        public void TestSimpleEcho()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            string wordToRepeat = "Bamba";
            string expectedResponse = "You said: " + wordToRepeat + ".";
            TestMethodsClass testClass = new TestMethodsClass();
            XmlDocument doc = new XmlDocument();
            FileStream fs = new FileStream("D:\\TestXml.xml", FileMode.Open, FileAccess.Read);
            doc.Load(fs);
            AIML bot = new AIML(testClass , client, doc);
            //string res = bot.Request("echo " + wordToRepeat, friendIdentity);

            client.PushMessage(friendIdentity, "echo " + wordToRepeat);
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);


            //client.PushMessage(friendIdentity, string.Format("echo  \"{0}\"", wordToRepeat));
            //Assert.AreEqual(expectedResponse, res);

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
            XmlDocument doc = new XmlDocument();
            FileStream fs = new FileStream("D:\\TestXml.xml", FileMode.Open, FileAccess.Read);
            doc.Load(fs);
            TestMethodsClass testClass = new TestMethodsClass();
            AIML bot = new AIML(testClass,client, doc);
            client.PushMessage(friendIdentity, string.Format("add {0} {1}", first, second));
            Assert.AreEqual(expectedResponse, client.SentMessageQueue.Dequeue().Item2);

        }

        [Test()]
        public void TestMovies()
        {
            Identity clientIdnetity = new Identity { UserName = "test", Domain = "test" };
            Identity friendIdentity = new Identity { UserName = "friend", Domain = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            XmlDocument doc = new XmlDocument();
            FileStream fs = new FileStream("D:\\TestXml.xml", FileMode.Open, FileAccess.Read);
            doc.Load(fs);
            TestMethodsClass testClass = new TestMethodsClass();
            AIML bot = new AIML(testClass, client, doc);
            string res = bot.Request("my name is maor", friendIdentity);
            res = bot.Request("I am 27 years old", friendIdentity);
            res = bot.Request("what is my name?", friendIdentity);
            client.PushMessage(friendIdentity, "give me a movie title with my name");

            //Assert.AreEqual(expectedResponse, res);

        }

    }
}