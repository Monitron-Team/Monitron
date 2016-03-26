using System;

namespace Monitron.ImRpc
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RemoteCommandAttribute : Attribute
    {
        public string MethodName { get; set; }
        public string Description { get; set; }
    }
}
