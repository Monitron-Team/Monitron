using System;

namespace Monitron.Plugins.LocalMonitorPlugin
{
	public sealed class PolicyViolatedEventArgs: EventArgs
	{
		public MonitoringPolicy MonitoringPolicy { get; private set; }
		public IMonitorConstraint MonitorConstraint { get; private set;}

		public PolicyViolatedEventArgs(MonitoringPolicy i_Policy, IMonitorConstraint i_Constraint): base()
		{
			MonitoringPolicy = i_Policy;
			MonitorConstraint = i_Constraint;
		}
	}
}

