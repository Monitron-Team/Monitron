using System;
using System.Diagnostics;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class MemoryMonitorConstraint: IMonitorConstraint
	{
		private Process m_Process;
		private int m_MaxPrivateMemorySetAllowed;

		public MemoryMonitorConstraint(int i_PID, int i_MaxPrivateMemorySetAllowed)
		{
			m_Process = Process.GetProcessById(i_PID);
			m_MaxPrivateMemorySetAllowed = i_MaxPrivateMemorySetAllowed;
		}

		public bool Check()
		{
			return m_Process.PrivateMemorySize64 > m_MaxPrivateMemorySetAllowed;
		}

		public string GetProblemDescrition()
		{
			return String.Format("Memory has reached size of {0}", m_MaxPrivateMemorySetAllowed.ToString());
		}
	}
}

