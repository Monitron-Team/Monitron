using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class MonitoringPolicy
	{
		private static readonly log4net.ILog sr_Log = log4net.LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private int m_ProcessPID;
		private string m_ProcessName;
		private IList<IMonitorConstraint> m_ProcessContraintsList;
		private ePolicyStatus m_Status;
		private const string k_MemoryConstraint = "memory";
		private const string k_CPUConstraint = "cpu";

		public MonitoringPolicy(int i_ProcessPID)
		{ 
			m_ProcessPID = i_ProcessPID;
			m_ProcessName = Process.GetProcessById(i_ProcessPID).ProcessName;
			m_ProcessContraintsList = new List<IMonitorConstraint>();
			m_Status = ePolicyStatus.ACTIVE;
		}

		public bool AddConstraint(string i_ConstraintType, int i_Size)
		{
			sr_Log.Debug("Adding " + i_ConstraintType + " constraint");
			bool succeeded = true;
			IMonitorConstraint newConstraint = null;

			switch(i_ConstraintType.ToLower()) 
			{
			case k_MemoryConstraint:
				newConstraint = new MemoryMonitorConstraint(m_ProcessPID, i_Size);
				break;
			case k_CPUConstraint:
				newConstraint = new CPUMonitorConstraint(m_ProcessPID, i_Size);
				break;
			default:
				succeeded = false;
				break;
			}

			if(newConstraint != null) 
			{
				m_ProcessContraintsList.Add(newConstraint);
			}

			return succeeded;
		}

		public IList<IMonitorConstraint> Check()
		{
			IList<IMonitorConstraint> violatedConstraints = new List<IMonitorConstraint>();

			if (m_Status == ePolicyStatus.ACTIVE)
			{
				foreach(IMonitorConstraint constraint in m_ProcessContraintsList) 
				{
					bool isViolated = constraint.Check();

					if(isViolated) 
					{
						violatedConstraints.Add(constraint);
					}
				}
			}

			return violatedConstraints;
		}
			
		public void Activate()
		{
			m_Status = ePolicyStatus.ACTIVE;
		}

		public void Disable()
		{
			m_Status = ePolicyStatus.DISABLE;
		}

		public override string ToString()
		{
			return string.Format("MonitoringPolicy: Name: {0}, PID: {1}, status: {2}", m_ProcessName, m_ProcessPID, m_Status);
		}

		public enum ePolicyStatus
		{
			ACTIVE = 0,
			DISABLE
		};
	}
}

