using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace tapi
{
    public class RawDataSource
    {
        public string Name { get;  }
        public List<DataFile> Files { get;  }
        private RawDataSource(string name, List<DataFile> files)
        {
            Name = name;
            Files = files;
        }
        public static RawDataSource Load(XElement xml)
        {
            var name = xml.Attribute("name")?.Value as string;
            var files = xml.Descendants(Model.XNAME_FILE).Select(x=>DataFile.Load(x)).ToList();
            return new RawDataSource(name,files);
        }
    }
}