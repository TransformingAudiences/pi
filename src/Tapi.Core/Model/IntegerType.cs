using System.Globalization;
using System.Xml.Linq;

namespace tapi
{
     public class IntegerType : IValueType
    {
        private IntegerType() {}
        public static IntegerType Load(XElement xml) => new IntegerType();
        public object Deserialize(string value)
        {
            return int.Parse(value);
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