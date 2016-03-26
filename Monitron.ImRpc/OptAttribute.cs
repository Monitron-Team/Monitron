using System;

namespace Monitron.ImRpc
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OptAttribute : Attribute
    {
        public string Name { get; set; }
        public string Prototype { get; set; }
        public string Description { get; set; }
        public OptAttribute(string i_Name, string i_Prototype, string i_Description)
        {
            Name = i_Name;
            Prototype = i_Prototype;
            Description = i_Description;
        }
    }
}

