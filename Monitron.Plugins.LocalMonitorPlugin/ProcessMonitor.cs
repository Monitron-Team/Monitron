using System;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class ProcessMonitor
	{
		private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		
		private Dictionary<int, MonitoringPolicy> m_MonitoringPoliciesDictionary;
		private Timer m_timer;
		public event EventHandler<PolicyViolatedEventArgs> PolicyViolated;
	
		public ProcessMonitor(int i_TimeInterval)
		{
			sr_Log.Info("Process Monitor is up");
			sr_Log.Debug("Process Monitor will check policies every " + i_TimeInterval.ToString() + " seconds");
			m_MonitoringPoliciesDictionary = new Dictionary<int, MonitoringPolicy>();
			m_timer = new Timer(
				e => checkAllMonitoringPolicies(),  
				null, 
				TimeSpan.Zero, 
				TimeSpan.FromSeconds(i_TimeInterval));
		}

		public bool StartMonitoring(int i_ProcessPID)
		{
			bool wasActivated = false;
			sr_Log.Debug("Activating Monitoring Process PID: " + i_ProcessPID);

			try
			{
				bool isNewPolicy = createNewMonitorPolicy(i_ProcessPID);
				if (!isNewPolicy)
				{
					m_MonitoringPoliciesDictionary[i_ProcessPID].Activate();
				}
				wasActivated = true;
			}
			catch(Exception e) 
			{
				sr_Log.ErrorFormat(e.Message);
			}
				
			return wasActivated;
		}

		public bool SuspendMonitoringByPID(int i_ProcessPID)
		{
			sr_Log.Debug("Suspending Monitoring Process PID: " + i_ProcessPID.ToString());
			bool wasSuspended = false;

			if (m_MonitoringPoliciesDictionary.ContainsKey(i_ProcessPID)) 
			{
				m_MonitoringPoliciesDictionary[i_ProcessPID].Disable();
				wasSuspended = true;
			}
			//process id doesn't exist
			else 
			{
				sr_Log.Debug("Could not suspend process PID : " + i_ProcessPID.ToString());
			}

			return wasSuspended;
		}

		public bool AddPolicy(int i_ProcessPID, string i_ConstraintType, int i_Size)
		{
			createNewMonitorPolicy(i_ProcessPID);
			bool succeeded = m_MonitoringPoliciesDictionary[i_ProcessPID].AddConstraint(i_ConstraintType, i_Size);

			return succeeded;
		}

		public Process[] GetMonitoringProccesses()
		{
			sr_Log.Debug("Getting all monitoring proccesses");
			Process[] monitoredProccesses = new Process[m_MonitoringPoliciesDictionary.Count];
			int count = 0;

			foreach(KeyValuePair<int, MonitoringPolicy> policy in m_MonitoringPoliciesDictionary) 
			{
				monitoredProccesses[count++] = Process.GetProcessById(policy.Key);
			}

			return monitoredProccesses;
		}

		protected void OnPolicyViolated(PolicyViolatedEventArgs e)
		{
			EventHandler<PolicyViolatedEventArgs> handler = PolicyViolated;

			if (handler != null)
			{
				sr_Log.Debug(string.Format("Notifying Listeners about a policy violated: {0}, {1}", 
					e.MonitoringPolicy.ToString(), e.MonitorConstraint.GetProblemDescrition()));
				handler(this, e);
			}
		}

		private void checkAllMonitoringPolicies()
		{
			sr_Log.Debug("Started checking monitoring policies");
			foreach (KeyValuePair<int, MonitoringPolicy> monitoringPolicyItem in m_MonitoringPoliciesDictionary)
			{
				IList<IMonitorConstraint> violatedConstraints = monitoringPolicyItem.Value.Check();

				sr_Log.Debug(String.Format("There are {0} Violated Constraints", violatedConstraints.Count));
				foreach(IMonitorConstraint constraint in violatedConstraints)
				{
					PolicyViolatedEventArgs args = new PolicyViolatedEventArgs(monitoringPolicyItem.Value, constraint);
					OnPolicyViolated(args);
				}
			}
			sr_Log.Debug("Finished checking monitoring policies");
		}

		private bool createNewMonitorPolicy(int i_ProcessPID)
		{
			bool createdNew = false;
			//create a new key in dictionary if hasn't been created yet
			if(!m_MonitoringPoliciesDictionary.ContainsKey(i_ProcessPID))
			{
				sr_Log.Debug("Creating new Monitoring Policy for PID: " + i_ProcessPID);
				m_MonitoringPoliciesDictionary.Add(i_ProcessPID, new MonitoringPolicy(i_ProcessPID));
				createdNew = true;
			}

			return createdNew;
		}
	}
}

