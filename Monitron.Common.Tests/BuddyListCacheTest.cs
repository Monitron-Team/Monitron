using System;

using NUnit.Framework;

using Monitron.Clients.Mock;
using Monitron.Common;

namespace Monitron.Common.Tests
{
    [TestFixture()]
    public class BuddyListCacheTest
    {
        [Test()]
        public void TestInitalLoad()
        {
            Identity clientIdnetity = new Identity{ UserName = "test", Host = "test" };
            Identity friendIdentity = new Identity(){ UserName = "friend", Host = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            client.BuddyDictionary.Add(friendIdentity, new string[]{ "admin" });
            BuddyListCache cache = new BuddyListCache(client);
            Assert.IsTrue(cache.Buddies.ContainsKey(friendIdentity));
            Assert.IsTrue(cache.Buddies[friendIdentity].Contains("admin"));
        }

        [Test()]
        public void TestBuddyAddition()
        {
            Identity clientIdnetity = new Identity{ UserName = "test", Host = "test" };
            Identity friendIdentity = new Identity(){ UserName = "friend", Host = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            BuddyListCache cache = new BuddyListCache(client);
            client.AddBuddy(friendIdentity, new string[]{ "admin" });
            Assert.IsTrue(cache.Buddies.ContainsKey(friendIdentity));
            Assert.IsTrue(cache.Buddies[friendIdentity].Contains("admin"));
        }

        [Test()]
        public void TestBuddyDeletion()
        {
            Identity clientIdnetity = new Identity{ UserName = "test", Host = "test" };
            Identity friendIdentity = new Identity(){ UserName = "friend", Host = "test" };
            MockMessengerClient client = new MockMessengerClient(clientIdnetity);
            client.BuddyDictionary.Add(friendIdentity, new string[]{ "admin" });
            BuddyListCache cache = new BuddyListCache(client);
            client.RemoveBuddy(friendIdentity);
            Assert.IsFalse(cache.Buddies.ContainsKey(friendIdentity));
        }
    }
}

