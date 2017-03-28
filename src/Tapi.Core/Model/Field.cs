using System;
using System.Xml.Linq;

namespace tapi
{
    public class Field
    {
        public IValueType ValueType { get; }
        public string Name { get; set; }
        public VariableType VariableType { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        private Field(string name, VariableType variableType, IValueType valueType, int position, int length)
        {
            Name = name;
            VariableType = variableType;
            ValueType = valueType;
            Position = position;
            Length = length;
        }
        public static Field Load(XElement xml)
        {
            var name =  xml.Attribute("name")?.Value as string;
            var variableType =  xml.Attribute("variableType")?.Value as string ?? "Variable";
            var position =  xml.Attribute("position")?.Value as string;
            var length =  xml.Attribute("length")?.Value as string;
            var valueType =  xml.Attribute("valueType")?.Value as string;
            var vt = valueType == "Option" ? OptionType.Load(xml) as IValueType :
                     valueType == "Timespan" ? TimespanType.Load(xml) as IValueType :
                     valueType == "Text" ? TextType.Load(xml) as IValueType :
                     valueType == "Double" ? DoubleType.Load(xml) as IValueType :
                     valueType == "Integer" ? IntegerType.Load(xml) as IValueType :
                     throw new ArgumentException($"Can't find valueType '{valueType}'");
            return new Field(
                name,
                (VariableType)Enum.Parse(typeof(VariableType),variableType),
                vt,
                int.Parse(position),
                int.Parse(length));
        }

    }
}