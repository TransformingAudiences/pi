using System.Xml.Linq;

namespace tapi
{
    public class TextType : IValueType
    {
        private TextType(){}
        public static TextType Load(XElement xml) => new TextType();
        public object Deserialize(string value)
        {
            return value;
        }

        public bool EqualTo(object first, object second)
        {
            return first == second;
        }

        public string ToDisplayText(object value)
        {
            return value as string;
        }
    }
}