using System;

namespace Monitron.AI
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteCommandAttribute : Attribute
    {
        public string MethodName { get; set; }
        public string Description { get; set; }
    }
}
