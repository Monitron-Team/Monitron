using System;
using Monitron.Common;


namespace Monitron.Common
{
	public interface INodePlugin
	{
		IMessengerClient MessangerClient { get; }
	}
}

