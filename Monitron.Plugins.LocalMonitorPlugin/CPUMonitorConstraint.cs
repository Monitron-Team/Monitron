using System;
using System.Diagnostics;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class CPUMonitorConstraint: IMonitorConstraint
	{
		private Process m_Process;
		private int m_MaxCPUAllowed;

		public CPUMonitorConstraint(int i_PID, int i_MaxCPUAllowed)
		{
			m_Process = Process.GetProcessById(i_PID);
			m_MaxCPUAllowed = i_MaxCPUAllowed;
		}

		public bool Check()
		{
			int osPlatform = (int)Environment.OSVersion.Platform;

			//checking if the os system is unix
			if((osPlatform == 4) || (osPlatform == 6) || (osPlatform == 128)) 
			{
				string cpuUsage = runCommandOnLinux(string.Format("ps -p <{0}> -o %cpu", m_Process.Id));
				return Convert.ToInt32(cpuUsage) > m_MaxCPUAllowed;

			} 
			//for testing purposes - most of our clients are windows
			else 
			{
				bool randomValue = false;
				Random random = new Random();
				if(random.Next() % 2 == 0 ) 
				{
					randomValue = true;
				}

				return randomValue;
			}
		}

		public string GetProblemDescrition()
		{
			return String.Format("CPU has reached size of {0}", m_MaxCPUAllowed.ToString());
		}

		private string runCommandOnLinux(string i_Command)
		{
			ProcessStartInfo procStartInfo = new ProcessStartInfo(i_Command);
			procStartInfo.RedirectStandardOutput = true;
			procStartInfo.UseShellExecute = false;
			procStartInfo.CreateNoWindow = true;

			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.StartInfo = procStartInfo;
			proc.Start();

			return proc.StandardOutput.ReadToEnd();
		}
	}
}

