using System;

namespace Monitron.ImRpc
{
    [AttributeUsage(AttributeTargets.Method)]  //todo: force for methods with only one argument string and static
    public class ArgumentParserAttribute : Attribute
    {
    }

}
