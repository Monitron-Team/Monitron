using System;
using System.Diagnostics;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public sealed class ProcessStatus
	{
		public int PID { get; private set; }
		public string Name { get; private set; }
		public eProcessState State { get; private set; }
		public long VmSize { get; private set; }
		public long VmRSS { get; private set; }
		public int Threads { get; private set; }

		static ProcessStatus()
		{
		}

		public static ProcessStatus[] GetProcessStatusByName(string i_ProcessName)
		{
			ProcessStatus[] processesStatus = null;
			Process[] processes = Process.GetProcessesByName(i_ProcessName);
			//found processes ByName given by user
			if(processes != null) 
			{
				processesStatus = new ProcessStatus[processes.Length];
				int i = 0;

				foreach(Process process in processes) 
				{
					ProcessStatus status = new ProcessStatus();
					status.PID = process.Id;
					status.Name = process.ProcessName;
					status.State = (process.Responding) ? eProcessState.RUNNING : eProcessState.NOTRESPONDING;
					status.VmSize = process.VirtualMemorySize64;
					status.VmRSS = process.WorkingSet64;
					status.Threads = process.Threads.Count;
					processesStatus[i++] = status;
				}
			}

			return processesStatus;
		}

		public override string ToString()
		{
			return string.Format("ProcessStatus: PID={0}, Name={1}, State={2}, VmSize={3}, VmRSS={4}, Threads={5}", 
				PID, Name, State, VmSize, VmRSS, Threads);
		}

		public enum eProcessState
		{
			RUNNING = 0,
			NOTRESPONDING
		};
	}
}

