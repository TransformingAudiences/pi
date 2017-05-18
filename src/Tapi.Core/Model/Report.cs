using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Globalization;

namespace tapi
{
    public class Report
    {
        public string Name { get; }
        public Repertoire Reportoar { get; }
        public DateTime From { get; }
        public DateTime To { get; }
        public ReportDimension Rows {get;}
        public ReportDimension Columns {get;}
        public PostProcessType PostProcess { get;  }
        public OutputFormat Format { get;  }
        public string Template { get;  }

        public int NbrOfDays => (To - From).Days + 1;
        public string FileName => Name + (Format == OutputFormat.Xlsx ? ".xlsx" : ".csv");
        private Report(string name, Repertoire reportoar, DateTime from, DateTime to, ReportDimension rows, ReportDimension columns, PostProcessType postprocess, OutputFormat format, string template)
        {
            Name = name;
            Reportoar = reportoar;
            From = from;
            To = to;
            Rows = rows;
            Columns = columns;
            PostProcess = postprocess;
            Format = format;
            Template = template;


        }
        public static Report Load(XElement xml, List<Repertoire> reportoires)
        {
            var name = xml.Attribute("name")?.Value as string;
            var reportoireName = xml.Attribute("reportoire")?.Value as string;
            var rowsIn = xml.Attribute("rows")?.Value as string;
            var columnsIn = xml.Attribute("columns")?.Value as string;
            
            var fromAttribute = xml.Attribute("from")?.Value as string;
            var toAttribute = xml.Attribute("to")?.Value as string;
            var aggregateAttribute = xml.Attribute("aggregate")?.Value as string;
            var postprocessAttribute = xml.Attribute("postprocess")?.Value as string;
            var formatAttribute = xml.Attribute("format")?.Value as string;
            var templateAttribute = xml.Attribute("template")?.Value as string;
            
            var reportoire = reportoires.Where(x=>x.Name == reportoireName).FirstOrDefault();
            var rows = (ReportDimension)Enum.Parse(typeof(ReportDimension), rowsIn);
            var columns = (ReportDimension)Enum.Parse(typeof(ReportDimension), columnsIn);
            
            var to = DateTime.ParseExact(toAttribute,"yyyy-MM-dd", CultureInfo.InvariantCulture);
            var from = DateTime.ParseExact(fromAttribute,"yyyy-MM-dd", CultureInfo.InvariantCulture);
            var format = (OutputFormat)Enum.Parse(typeof(OutputFormat), formatAttribute);
            var postprocess = (PostProcessType)Enum.Parse(typeof(PostProcessType), postprocessAttribute);
            var template = templateAttribute ?? "";

            if((rows == ReportDimension.Period || columns ==  ReportDimension.Period) && !(rows == ReportDimension.Time || columns ==  ReportDimension.Time))
            {
                throw new ArgumentException($"In report {name}, Period can not be used without Time in rows, columns");
            }

            return new Report(name,reportoire,from,to,rows,columns,postprocess,format,template);
        }
    }
}