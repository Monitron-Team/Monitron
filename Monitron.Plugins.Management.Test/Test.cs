using System;
using System.Linq;

using NUnit.Framework;
using DigitalOcean.API;

using Monitron.Clients.Mock;
using Monitron.Common;
using DigitalOcean.API.Models.Requests;

namespace Monitron.Plugins.Management.Test
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void TestEcho()
        {
            string expectedText = "123";
            Identity friend = new Identity { UserName = "friend", Domain = "test.com" };
            MockMessengerClient client = new MockMessengerClient(new Identity());
            new ManagementPlugin(client, null);
            client.PushMessage(friend, "echo " + expectedText);
            Assert.AreEqual(client.SentMessageQueue.Dequeue().Item2, expectedText);
        }
    }
}

