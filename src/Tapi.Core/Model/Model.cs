using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;
using System.Xml;
using System.Xml.Schema;

namespace tapi
{
    public class Model
    {
        public const string XMLNS_TAPI = "http://tempuri.org/tapi.xsd";
        public static XName XNAME_RAWDATASOURCE = XName.Get("DataSource", XMLNS_TAPI);
        public static XName XNAME_FILE = XName.Get("File", XMLNS_TAPI);
        public static XName XNAME_FIELD = XName.Get("Field", XMLNS_TAPI);
        public static XName XNAME_CONFIGURATION = XName.Get("Configuration", XMLNS_TAPI);
        public static XName XNAME_OPTION = XName.Get("Option", XMLNS_TAPI);
        public static XName XNAME_REPORTOIRE = XName.Get("Repertoire", XMLNS_TAPI);
        public static XName XNAME_REPORT = XName.Get("Report", XMLNS_TAPI);

        public RawDataSource DataSource { get; }
        public List<Repertoire> Reportoires { get; }
        public List<Report> Reports { get; }
        public DateTime From => Reports.Min(x => x.From);
        public DateTime To => Reports.Max(x => x.To);
        private Model(RawDataSource sources, List<Repertoire> reportoires, List<Report> reports)
        {
            DataSource = sources;
            Reportoires = reportoires;
            Reports = reports;
        }
        public static Model Load(string filePath)
        {
            var file = File.ReadAllText(filePath);

            XElement xml = XElement.Parse(file);
            ValidateXml(xml);
            var source = xml.Descendants(XNAME_RAWDATASOURCE).Select(x => RawDataSource.Load(x)).FirstOrDefault();
            var reportoires = xml.Descendants(XNAME_REPORTOIRE).Select(x => Repertoire.Load(x)).ToList();
            var reports = xml.Descendants(XNAME_REPORT).Select(x => Report.Load(x, reportoires)).ToList();
            return new Model(source, reportoires, reports);
        }

        private static void ValidateXml(XElement xml)
        {
            if (File.Exists(@"tapi.xsd"))
            {
                // current .net core does not support xml Schema validation
            }
        }
    }
}
