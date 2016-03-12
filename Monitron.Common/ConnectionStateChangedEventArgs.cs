using System;
using System.Collections.Generic;
using System.IO;

namespace Monitron.Common
{
	public class ConnectionStateChangedEventArgs : EventArgs
	{
        public bool IsConnected { get; private set; }

        public ConnectionStateChangedEventArgs(bool i_IsConnected) : base()
        {
            IsConnected = i_IsConnected;
        }
	}
}

