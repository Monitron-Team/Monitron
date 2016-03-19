using NUnit.Framework;
using System;
using System.Threading;
using System.Diagnostics;

using Monitron.Common;
using Monitron.Clients.Mock;
using Monitron.Plugins.LocalMonitorPlugin;
using Monitron.PluginDataStore.Local;

namespace Monitron.Plugins.LocalMonitorPlugin.Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void TestGetMemoryStatus()
		{
			Identity friend = new Identity { UserName = "friend", Domain = "test.com" };
			IPluginDataStore dataStore = new LocalPluginDataStore("localdatastore.json");
			MockMessengerClient client = new MockMessengerClient(new Identity());
			new LocalMonitorPlugin(client, dataStore);
			client.PushMessage(friend, "get_memory_status");
			string messageRecieved = client.SentMessageQueue.Dequeue().Item2;
			Assert.IsTrue(messageRecieved.Contains("Used"));
			Assert.IsTrue(messageRecieved.Contains("Free"));
			Assert.IsTrue(messageRecieved.Contains("Total"));
		}

		[Test()]
		public void TestGetProcessStatus()
		{
			string expectedText = "There aren't any processes matching: TestProcess";
			Identity friend = new Identity { UserName = "friend", Domain = "test.com" };
			IPluginDataStore dataStore = new LocalPluginDataStore("localdatastore.json");
			MockMessengerClient client = new MockMessengerClient(new Identity());
			new LocalMonitorPlugin(client, dataStore);
			client.PushMessage(friend, "get_process_status TestProcess");
            Assert.AreEqual(expectedText, client.SentMessageQueue.Dequeue().Item2);
		}

		[Test()]
		public void TestListMonitoredProcesses()
		{
			string expectedText = "There are no Monitoring proccesses";
			Identity friend = new Identity { UserName = "friend", Domain = "test.com" };
			IPluginDataStore dataStore = new LocalPluginDataStore("localdatastore.json");
			MockMessengerClient client = new MockMessengerClient(new Identity());
			new LocalMonitorPlugin(client, dataStore);
			client.PushMessage(friend, "list_monitored_processes");
            Assert.AreEqual(expectedText, client.SentMessageQueue.Dequeue().Item2);
		}

		[Test()]
		public void TestMonitoProcessStart()
		{
            int PID = Process.GetCurrentProcess().Id;
			string expectedText = "Started monitoring process PID " + PID;
			Identity friend = new Identity { UserName = "friend", Domain = "test.com" };
			IPluginDataStore dataStore = new LocalPluginDataStore("localdatastore.json");
			MockMessengerClient client = new MockMessengerClient(new Identity());
			new LocalMonitorPlugin(client, dataStore);
			client.PushMessage(friend, "monitor_process_start " + PID);
            Assert.AreEqual(expectedText, client.SentMessageQueue.Dequeue().Item2);
		}
	}
}

