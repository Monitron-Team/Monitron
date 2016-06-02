using System;

using Monitron.Common;

using S22.Xmpp.Extensions;

namespace Monitron.Clients.XMPP
{
    public class FileTransferAdapter : IFileTransfer
    {
        private readonly FileTransfer r_FileTransfer;

        internal FileTransferAdapter(FileTransfer i_FileTransfer)
        {
            r_FileTransfer = i_FileTransfer;
        }

        public string Id {
            get
            {
                return r_FileTransfer.SessionId;
            }
        }
        
        public Identity From
        {
            get
            {
                return r_FileTransfer.From.ToIdentity();
            }
        }

        public Identity To
        {
            get
            {
                return r_FileTransfer.To.ToIdentity();
            }
        }

        public string Name
        {
            get
            {
                return r_FileTransfer.Name;
            }
        }

        public long Size
        {
            get
            {
                return r_FileTransfer.Size;
            }
        }

        public long Transferred
        {
            get
            {
                return r_FileTransfer.Transferred;
            }
        }


        internal FileTransfer InternalFileTransfer
        {
            get
            {
                return r_FileTransfer;
            }
        }
    }
}

