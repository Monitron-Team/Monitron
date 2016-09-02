using System;

namespace Monitron.Plugins.Kodi
{
	public class MessageArrivedEventArgs: EventArgs
	{
		public string Message { get; private set; }
		public string ReturnValue { get; set; }

		public MessageArrivedEventArgs(string i_Message) : base()
		{
			Message = i_Message;
			ReturnValue = String.Empty;
		}
	}
}

