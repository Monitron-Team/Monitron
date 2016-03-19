using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public class MemoryStatus
	{
        private const ulong k_UnknownMessage = 0;
        public ulong Used { get; private set; }
        public ulong Free { get; private set;}
        public ulong Total { get; private set; }
        public ulong Cached { get; private set; }

		public MemoryStatus()
		{
			int osPlatform = (int)Environment.OSVersion.Platform;

			//checking if the os system is unix
			if((osPlatform == 4) || (osPlatform == 6) || (osPlatform == 128)) {
                Dictionary<string, ulong> memInfo = readMeminfo();
                Total = memInfo["MemTotal:"];
                Free = memInfo["MemFree:"];
                Used = Total - Free;
                Cached = memInfo["Cached:"];
			} 
			else 
			{
				Used = k_UnknownMessage;
				Free = k_UnknownMessage;
				Total = k_UnknownMessage;
			}
		}

        private Dictionary<string, ulong> readMeminfo()
        {
            Dictionary<string, ulong> result = new Dictionary<string, ulong>();
            foreach (string line in File.ReadLines("/proc/meminfo"))
            {
                string[] splitLine = line.Split(new char[0]{}, StringSplitOptions.RemoveEmptyEntries);
                ulong value = ulong.Parse(splitLine[1]);
                if (splitLine.Length == 2)
                {
                    value /= 1024;
                }

                result.Add(splitLine[0].Trim(), value);
            }

            return result;
        }

		public override string ToString()
		{
            return string.Format("MemoryStatus: Used={0} KB, Free={1} KB, Total={2} KB, Cached={3} KB", 
				Used, Free, Total, Cached);
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

