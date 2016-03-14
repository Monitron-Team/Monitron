using System;
using System.ComponentModel;

namespace Monitron.ImRpc.Tests
{
    [TypeConverter(typeof(ConvertableTypeConverter))]
    public class ConvertableType
    {
        public string String { get; set; }
        public int Number { get; set; }
    }
}

