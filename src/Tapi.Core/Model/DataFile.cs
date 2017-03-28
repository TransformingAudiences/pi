using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace tapi
{
    public class DataFile
    {
        public string Name {get;}
        public string Type { get; set; }
        public string FileName { get; set; }
        public List<Field> Fields { get; set; }
        private DataFile(string name, string type, string fileName, List<Field> fields)
        {
            Name = name;
            Type = type;
            FileName = fileName;
            Fields = fields;    
        }
        public static DataFile Load(XElement xml)
        {
            var name = xml.Attribute("name")?.Value as string;
            var type = xml.Attribute("type")?.Value as string;
            var fileName = xml.Attribute("fileName")?.Value as string;
            var fields = xml.Descendants(Model.XNAME_FIELD).Select(x=>Field.Load(x)).ToList();
            return new DataFile(name, type,fileName,fields);
        }
    }
}