﻿using System;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using Monitron.Common;
using Monitron.ImRpc;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class LocalMonitorPlugin: INodePlugin
	{
		private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private readonly IMessengerClient r_Client;

		public IMessengerClient MessangerClient
		{
			get
			{
				return r_Client;
			}
		}

		private readonly RpcAdapter r_Adapter;
		private ProcessMonitor m_ProcessMonitor;
		private IPluginDataStore m_DataStore;

		public LocalMonitorPlugin(IMessengerClient i_MessangerClient, IPluginDataStore i_DataStore)
		{
			sr_Log.Info("Local Plugin Starting");
			r_Client = i_MessangerClient;
			m_DataStore = i_DataStore;
			sr_Log.Debug("Setting up rpc");
			r_Adapter = new RpcAdapter(this, r_Client);
			sr_Log.Debug("Setting up Process Monitor");
			int timeIntervalPolicyCheck = m_DataStore.Read<int>("TimeIntervalPolicyCheck");
			m_ProcessMonitor = new ProcessMonitor(timeIntervalPolicyCheck);
			m_ProcessMonitor.PolicyViolated += m_ProcessMonitor_PolicyViolated;
			r_Client.ConnectionStateChanged += r_Client_ConnectionStateChanged;
		}

		private void r_Client_ConnectionStateChanged (object sender, ConnectionStateChangedEventArgs e)
		{
			sr_Log.Debug("Notifying buddies that I am running");

			if(e.IsConnected) 
			{
				sr_Log.Debug("Setting up nickname");
				string nickname = "Local_" + r_Client.Identity.UserName;
				r_Client.SetNickname("Local_" + r_Client.Identity.UserName);
				sr_Log.Debug("Setting up avatar");
				r_Client.SetAvatar(Assembly.GetExecutingAssembly().GetManifestResourceStream("Monitron.Plugins.LocalMonitorPlugin.LocalAvatar.png"));

				foreach(var buddy in r_Client.Buddies) 
				{
					string welcomeMessange = "\nI am running. Waiting for your commands:";
					r_Client.SendMessage(buddy.Identity, welcomeMessange);
				}
			}
		}

		private void m_ProcessMonitor_PolicyViolated(object sender, PolicyViolatedEventArgs e)
		{
			string message = string.Format("{0} - {1}",
				e.MonitoringPolicy.ToString(), e.MonitorConstraint.GetProblemDescrition());

			sr_Log.Debug("Notifying buddies that policy has been violated: " + message);
			foreach (var buddy in r_Client.Buddies)
			{
				r_Client.SendMessage(buddy.Identity, message);
			}
		}

		[RemoteCommand(MethodName="get_memory_status")]
        public string GetMemoryStatus(Identity i_Buddy)
		{
			sr_Log.Info("Getting Memory Status"); 
			string memoryStatus = new MemoryStatus().ToString();

			sr_Log.Debug("Memory Status is: " + memoryStatus);
			return memoryStatus;
		}

		[RemoteCommand(MethodName="get_process_status")]
        public string GetProcessStatus(Identity i_Buddy, string i_ProcessName)
		{
			string allProccesses = String.Empty;
			int index = 1;

			sr_Log.Info("Getting Process Status of " + i_ProcessName); 
			ProcessStatus[] processes = ProcessStatus.GetProcessStatusByName(i_ProcessName);

			foreach(ProcessStatus process in processes) 
			{
				allProccesses += String.Format("{0}. {1}\n", index++, process.ToString());
			}
		
			if(processes.Length == 0) 
			{
				allProccesses = "There aren't any processes matching: " + i_ProcessName;
			}
	 
			return allProccesses;
		}

		[RemoteCommand(MethodName="monitor_process_start")]
        public string MonitorProcessStart(Identity i_Buddy, int i_ProcessPID)
		{
			string message = String.Format("Started monitoring process PID {0}", i_ProcessPID.ToString());
			bool succeeded = m_ProcessMonitor.StartMonitoring(i_ProcessPID);

			if(!succeeded) 
			{
				message = String.Format("could start monitoring Process PID {0}", i_ProcessPID.ToString());
			}

			return message;
		}

		[RemoteCommand(MethodName="monitor_process_stop")]
        public string MonitorProcessStop(Identity i_Buddy, int i_ProcessPID)
		{
			string message = String.Format("process PID {0} was suspended successfully", i_ProcessPID.ToString());
			bool succeeded = m_ProcessMonitor.SuspendMonitoringByPID(i_ProcessPID);

			if(!succeeded) 
			{
				message = String.Format("process PID {0} could not be suspended", i_ProcessPID.ToString());
			}

			return message;
		}

		[RemoteCommand(MethodName="add_policy")]
        public string MonitorProcessAddPolicy(Identity i_Buddy, int i_ProcessPID, string i_PolicyType, int i_Size)
		{
			string message = String.Empty;

			bool succeeded = m_ProcessMonitor.AddPolicy(i_ProcessPID, i_PolicyType, i_Size);
			if(succeeded) 
			{
				message += String.Format("{0} Policy for Process PID: {1} was added successfully", 
					i_PolicyType, i_ProcessPID);
			} 
			else 
			{
				message += String.Format("{0} Policy for Process PID: {1} could not be added", 
					i_PolicyType, i_ProcessPID);
			}

			return message;
		}

		[RemoteCommand(MethodName="list_monitored_processes")]
        public string GetListMonitoringProcesses(Identity i_Buddy)
		{
			sr_Log.Debug("Getting list of all monitoring proccesses");
			string allMonitoringProcessesString = String.Empty;
			Process[] proccesses = m_ProcessMonitor.GetMonitoringProccesses();

			if(proccesses.Length > 0) 
			{
				int index = 1;

				foreach(Process process in proccesses) 
				{
					allMonitoringProcessesString += String.Format("{0}. {1}: {2}\n", 
						index, process.Id.ToString(), process.ProcessName);
				}
			} 
			else 
			{
				allMonitoringProcessesString = "There are no Monitoring proccesses";
			}

			sr_Log.Debug("List of all monitoring proccesses: " + allMonitoringProcessesString);
			return allMonitoringProcessesString;
		}
	}
}

