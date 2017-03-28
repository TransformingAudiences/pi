using System.Globalization;
using System.Xml.Linq;

namespace tapi
{
     public class DoubleType : IValueType
    {
        private DoubleType() {}
        public static DoubleType Load(XElement xml) => new DoubleType();
        public object Deserialize(string value)
        {
            return double.Parse(value, new NumberFormatInfo {NumberDecimalSeparator = "."  });
        }

        public bool EqualTo(object first, object second)
        {
            return first == second;
        }

        public string ToDisplayText(object value)
        {
            return value?.ToString();
        }
    }
}