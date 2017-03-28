using System.Xml.Linq;

namespace tapi
{
    public sealed class OptionValue
    {
        public string Name { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public int Value { get; private set; }
        public int Index => Value;
        public OptionValue(string name, string displayName, string description, int value)
        {
            Name = name;
            DisplayName = string.IsNullOrEmpty(displayName) ? Name : displayName;
            Description = description;
            Value = value;
        }

        public static OptionValue Load(XElement xml)
        {
            var name =  xml.Attribute("name")?.Value as string;
            var displayName =  xml.Attribute("displayName")?.Value as string;
            var description =  xml.Attribute("description")?.Value as string;
            var value =  xml.Attribute("value")?.Value as string;
            return new OptionValue(name,displayName,description,int.Parse(value));
        }
    }
}