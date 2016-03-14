using System;
using System.ComponentModel;
using System.Globalization;

namespace Monitron.ImRpc.Tests
{
    public class ConvertableTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string)
            {
                string svalue = (string)value;
                string[] parts = svalue.Split(';');
                return new ConvertableType
                {
                    String = parts[0],
                    Number = int.Parse(parts[1])
                };
            }
            else
            {
                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

