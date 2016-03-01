using System;
using Monitron.Common;


namespace Monitron.Common
{
	public interface IPlugin
	{
		IMessengerClient MessangerClient { get; }

	}
}

