using System;
using System.Collections.Generic;
using System.IO;

namespace Monitron.Common
{
	public class FileTransferProgressEventArgs : EventArgs
	{
        public IFileTransfer FileTransfer { get; private set; }

        public FileTransferProgressEventArgs(IFileTransfer i_FileTransfer)
        {
            FileTransfer = i_FileTransfer;
        }
	}
}

