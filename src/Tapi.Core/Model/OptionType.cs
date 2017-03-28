using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace tapi
{
    public class OptionType : IValueType
    {
        public Dictionary<string,OptionValue> Options { get; set; }
        public bool IsMultipleChoice { get; set; }
        private OptionType(bool isMultipleChoice, List<OptionValue> options)
        {
            IsMultipleChoice = isMultipleChoice;
            Options = options.ToDictionary(x=>x.Name);
        }
        public static OptionType Load(XElement xml)
        {
            var ismulti = xml.Descendants(Model.XNAME_CONFIGURATION)
                             .Where(x=> x.Attribute("name")?.Value == "ValueType.Option.IsMultipleChoice")
                             .Select(x=>x.Attribute("value")?.Value)
                             .FirstOrDefault() ?? "false";
            var options = xml.Descendants(Model.XNAME_OPTION)
                             .Select(x=> OptionValue.Load(x))
                             .ToList();                            
            return new OptionType(bool.Parse(ismulti), options);
        }

        public object Deserialize(string value)
        {
            var i  = 0;
            var c = value.Length == 1 ? (int)value[0] : -1;
            var isInt = int.TryParse(value, out i) || c >= 0;
            return Options.ContainsKey(value) ?  Options[value] : 
                   isInt ? Options.Values.Where(x=> x.Value == i || x.Value == c).Single() :
                   throw new ArgumentException($"Can't find the option value '{value}'");
        }

        public string ToDisplayText(object value)
        {
            return value is OptionValue  s? s.DisplayName :
                   value is MultipleOption  m? string.Join(", ", m.Values) :
                   null; 
        }

        public bool EqualTo(object first, object second)
        {
            var f = first as OptionType;
            var s = second as OptionType;
            return first == second && first != null;
        }
    }
}