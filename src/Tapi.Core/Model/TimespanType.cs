using System;
using System.Linq;
using System.Xml.Linq;

namespace tapi
{
     public class TimespanType : IValueType
    {
        public string Format { get; set; }
        private TimespanType(string format)
        {
            Format = format;
        }
        public static TimespanType Load(XElement xml)
        {
            var format = xml.Descendants(Model.XNAME_CONFIGURATION)
                            .Where(x=> x.Attribute("name")?.Value == "ValueType.Timespan.Format")
                            .Select(x=>x.Attribute("value")?.Value)
                            .FirstOrDefault() ?? "HHmmss";
            return new TimespanType(format);
        }

        public object Deserialize(string value)
        {
            return TimeSpan.Parse(value);
        }

        public bool EqualTo(object first, object second)
        {
            var f = first as OptionType;
            var s = second as OptionType;
            return first == second && first != null;
        }

        public string ToDisplayText(object value)
        {
            return value.ToString();
        }
    }
}