using System;
using System.Collections.Generic;

namespace Monitron.Discovery
{
	public interface IDiscovery
	{
		List<String> GetRegisteredInterfaces();
	}
}

