using System;

using NUnit.Framework;

using Monitron.PluginDataStore.Cloud;

namespace Monitron.PluginDataStore.Cloud.Tests
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void TestCase()
        {
            CloudPluginDataStore ds = new CloudPluginDataStore(
                @"mongodb://monitron_mgmt:HellIsForHeroes%40!@boss.monitron.test/monitron");
            const string key = "key";
            TestStruct s = new TestStruct
            {
                    s = "string",
                    i = 42,
            };
            ds.Write(key, s);
            TestStruct res = ds.Read<TestStruct>(key);
            Assert.AreEqual(s.s, res.s);
            Assert.AreEqual(s.i, res.i);
            s = new TestStruct
                {
                    s = "sstring",
                    i = 43,
                };
            ds.Write(key, s);
            res = ds.Read<TestStruct>(key);
            Assert.AreEqual(s.s, res.s);
            Assert.AreEqual(s.i, res.i);
            ds.Delete(key);
        }
    }
}

