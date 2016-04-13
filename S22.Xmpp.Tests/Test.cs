using NUnit.Framework;
using System;
using S22.Xmpp.Client;

namespace S22.Xmpp.Tests
{
    [TestFixture()]
    public class Test
    {
        [Test()]
        public void TestMissingServer()
        {
        }

        [Test()]
        public void TestCommandExecution()
        {
            XmppClient client = new XmppClient(
                "monitron-boss",
                "admin",
                "NoRestForTheWicked@@@"
            );
            client.Connect();
            IRpcTestInterface rpcClient = client.CreateJabberRpcClient<IRpcTestInterface>(client.Jid);
            client.RegisterJabberRpcServer<IRpcTestInterface, RpcImplementation>(new RpcImplementation());
            rpcClient.doNothing();
            Assert.AreEqual("Hello", rpcClient.echo("Hello"));
            Assert.AreEqual("HelloWorld", rpcClient.concat("Hello", "World"));
            Assert.AreEqual(10, rpcClient.sum(5, 5));
            Assert.AreEqual(true, rpcClient.isTrue(true));
            Assert.AreEqual(2d, rpcClient.div(10d, 5d));
            TestStruct ts = new TestStruct
            {
                StringField = "Hello",
                IntField = 3,
                DoubleField = 10d
            };
            TestStruct res = rpcClient.echoStruct(ts);
            Assert.AreEqual(ts.IntField, res.IntField);
            Assert.AreEqual(ts.StringField, res.StringField);
        }
    }
}

