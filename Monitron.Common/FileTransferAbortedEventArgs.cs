using System;
using System.Collections.Generic;
using System.IO;

namespace Monitron.Common
{
	public class FileTransferAbortedEventArgs : EventArgs
	{
        public IFileTransfer FileTransfer { get; private set; }

        public FileTransferAbortedEventArgs(IFileTransfer i_FileTransfer)
        {
            FileTransfer = i_FileTransfer;
        }
	}
}

