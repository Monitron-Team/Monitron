using System;

namespace Monitron.Common
{
    public sealed class MessageArrivedEventArgs : EventArgs
    {
        public Identity Buddy { get; private set; }
        public string Message { get; private set; }

        public MessageArrivedEventArgs(Identity i_Buddy, string i_Message) : base()
        {
            this.Buddy = i_Buddy;
            this.Message = i_Message;
        }
    }
}

