using System;
using System.Diagnostics;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class MemoryStatus
	{
		private const string k_UnknownMessage = "Unknown";
		public string Used { get; private set; }
		public string Free { get; private set;}
		public string Total { get; private set; }
		public string Shared { get; private set; }
		public string Cached { get; private set; }
		public string Available { get; private set; }

		public MemoryStatus()
		{
			int osPlatform = (int)Environment.OSVersion.Platform;

			//checking if the os system is unix
			if((osPlatform == 4) || (osPlatform == 6) || (osPlatform == 128)) {
				Used = runCommandOnLinux("cat /proc/meminfo | grep Active | cut -d':' -f2"); //It gives me 3 kind of active
				Free = runCommandOnLinux("cat /proc/meminfo | grep MemFree | cut -d':' -f2");
				Total = runCommandOnLinux("cat /proc/meminfo | grep MemTotal | cut -d':' -f2");
				Shared = "?"; 
				Cached = runCommandOnLinux("cat /proc/meminfo | grep Cached | cut -d':' -f2");
				Available = "?";
			} 
			else 
			{
				Used = k_UnknownMessage;
				Free = k_UnknownMessage;
				Total = k_UnknownMessage;
				Shared = k_UnknownMessage;
				Cached = k_UnknownMessage;
				Available = k_UnknownMessage;
			}
		}

		public override string ToString()
		{
			return string.Format("MemoryStatus: Used={0}, Free={1}, Total={2}, Shared={3}, Cached={4}, Available={5}", 
				Used, Free, Total, Shared, Cached, Available);
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

