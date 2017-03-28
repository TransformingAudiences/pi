using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace tapi
{
    public class CommandLineParser
        {
            public static object Parse(string[] args)
            {
                var items = args.Select(arg=>{
                    var index = arg.IndexOf(":");
                    if(index < 0)
                    {
                        return new {Name = arg.Trim('-'), Data = ""};
                    }
                    var name = arg.Substring(0,index).Trim('-');
                    var data = arg.Substring(index + 1).Trim('"');
                    return new {Name = name, Data = data};
                });

                var duplicates = items.GroupBy(x=>x.Name).Where(x=>x.Count() > 1).Select(x=>x.Key).ToList();
                if(duplicates.Any())
                {
                    return new Error { Message = "Dupliacated input arguments " + string.Join(", ", duplicates) };
                }
                var map = items.ToDictionary(x=>x.Name.ToLower(), x=> x.Data);
                if(map.ContainsKey("help"))
                {
                    return new Help();
                }

                var requiredNames = new string[]{"conf","data","templates","out"};
                var missing = requiredNames.Where(x=> !map.Keys.Contains(x));
                if(missing.Any())
                {
                    return new Error { Message = "Missing input arguments " + string.Join(", ", missing) };
                }

                return new ExecuteReports(map);
            }

            public class ExecuteReports
            {
                public string ModelFile { get;  }
                public string DataDir { get;  }
                public string TemplateDir { get;  }
                public string OutputDir { get;  }
                public LogLevel LogLevel {get;}
                public ExecuteReports(Dictionary<string,string> map)
                {
                    ModelFile = map["conf"];
                    DataDir = map["data"];
                    TemplateDir = map["templates"];
                    OutputDir = map["out"];
                    LogLevel = Enum.TryParse<LogLevel>(map.ContainsKey("log") ? map["log"] : "", out LogLevel level ) 
                        ? level 
                        : LogLevel.Normal;
                }
            }
            public class Help
            {
                
            }
            public class Error
            {
                public string Message { get; set; }
            }
        }
    
}