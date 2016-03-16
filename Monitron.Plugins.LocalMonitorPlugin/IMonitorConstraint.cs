using System;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public interface IMonitorConstraint
	{
		bool Check();
		string GetProblemDescrition();
	}
}

