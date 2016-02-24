using System;
using System.Configuration;
using System.Threading;
using System.Linq;

using NUnit.Framework;

using Monitron;
using Monitron.Common;
using Monitron.Clients.XMPP;
using System.Collections.Generic;

namespace Monitron.Clients.XMPP.Tests
{
    [TestFixture()]
    public class Test
    {
        private Account getTestAccount(string i_Name)
        {
            var appSettings = ConfigurationManager.AppSettings;
            Account account = new Account(
                                  i_UserName: appSettings[i_Name + "_username"],
                                  i_Password: appSettings[i_Name + "_password"],
                                  i_Host: appSettings[i_Name + "_host"]
                              );

            return account;
        }

        private XMPPMessengerClient getTestClient(string i_Name)
        {
            return new XMPPMessengerClient(
                getTestAccount(i_Name)
            );
        }

        [Test()]
        public void TestConnect()
        {
            // This will implicitly connect
            getTestClient("TestUser01");
        }

        [Test()]
        public void SendMessageTest()
        {
            TimeSpan testTimeout = TimeSpan.FromSeconds(30);
            AutoResetEvent waitHandle = new AutoResetEvent(false);
            XMPPMessengerClient client1 = getTestClient("TestUser01");
            XMPPMessengerClient client2 = getTestClient("TestUser02");

            client2.MessageArrived += delegate(object sender, MessageArrivedEventArgs e)
            {
                    waitHandle.Set();
            };
            
            client1.sendMessage(client2.Identity, "Hello");

            Assert.IsTrue(waitHandle.WaitOne(testTimeout));
        }

        [Test()]
        public void BuddyListModificationTest()
        {
            TimeSpan buddyListUpdateSleepTimeSpan = TimeSpan.FromSeconds(1);
            TimeSpan testTimeout = TimeSpan.FromSeconds(30);
            XMPPMessengerClient client1 = getTestClient("TestUser01");
            Account account2 = getTestAccount("TestUser02");

            clearBuddyList(client1);
            client1.AddBuddy(account2.Identity);
            // It takes a while for the buddy to be added in the server
            // This loop will wait until it happens or we time out
            DateTime start = DateTime.Now;
            while (DateTime.Now - start < testTimeout)
            {
                if (client1.Buddies.Count() != 0)
                {
                    break;
                }

                Thread.Sleep(buddyListUpdateSleepTimeSpan);
            }
            Assert.AreEqual(1, client1.Buddies.Count());
            Assert.AreEqual(account2.Identity, client1.Buddies.First().Identity);
        }

        private void clearBuddyList(IMessengerClient i_Client)
        {
            foreach (BuddyListItem item in i_Client.Buddies.ToList())
            {
                i_Client.RemoveBuddy(item.Identity);
            }
        }
    }
}
