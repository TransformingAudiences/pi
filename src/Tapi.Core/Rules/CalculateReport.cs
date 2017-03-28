using System.Collections.Generic;
using System.Linq;
using System.IO;
using OfficeOpenXml;
using System;
using System.Text;
using Matrix = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, double>>;

namespace tapi
{
    public class CalculateReport
    {
         public (Matrix res,byte[] file) Execute(Report report, List<Event> events, string templateDir)
        {
            
            var res =
                events
                    .Where(e => report.From.Date <= e.Date && e.Date <= report.To.Date)
                    .Where(e => report.Reportoar.Filter(e))
                    .SelectMany(e => ExpandOnRepertoire(e, report.Reportoar))
                    .GroupBy(r => r.key)
                    .Select(r => new
                    {
                        r.Key,
                        Value =  CalculateWeightedReportoireValue(r.Key.time,report, r.Select(y => y.@event))

                    })
                    .GroupBy(x => CalculateAggregationKey(x.Key, report))
                    .Select(x => new
                    {
                        x.Key.row,
                        x.Key.column,
                        Value = report.Aggregeate == AggregateType.Count ? x.Count() :
                                report.Aggregeate == AggregateType.Sum ? x.Sum(y => y.Value) :
                                report.Aggregeate == AggregateType.Average ? x.Average(y => y.Value) :
                                throw new ArgumentException()
                    })
                    .GroupBy(x => x.row)
                    .ToDictionary(
                        x => x.Key,
                        x => x.ToDictionary(
                            y => y.column, 
                            y => report.Aggregeate == AggregateType.Sum && report.Rows != ReportDimension.Period && report.Columns != ReportDimension.Period
                             ? y.Value / report.NbrOfDays
                             : y.Value)
                    );

            res = report.PostProcess == PostProcessType.None ? res :
                  report.PostProcess == PostProcessType.VolumePercentage ? TransformVolumePercentage(res) :
                  report.PostProcess == PostProcessType.Rank ? TransformRank(res) :
                  throw new ArgumentException();

            res = AddZeroCells(res,report,events);

            var csv = CreateCsv(res);
            Logger.Log("\r\n" + PrittyPrint(res) +"\r\n", true,true);

            var file = report.Format == OutputFormat.Xlsx ? CreateExcel(res,report,templateDir) :
                       report.Format == OutputFormat.Csv ? Encoding.UTF8.GetBytes(csv) : 
                       throw new ArgumentException();

            return (res,file);
        }

        private double CalculateWeightedReportoireValue(string time, Report report, IEnumerable<Event> events)
        {
            var days = events.Select(x => x.Date.Date).Distinct().ToList();

            var weightMap = events.Select(x => x.Individual)
                    .GroupBy(x => x.Id)
                    .Select(x =>
                    {
                       
                        var i = x.First();
                        var weights = days.Where(y => i.Weights.ContainsKey(y)).Select(y => i.Weights[y]);
                        return new {x.Key, Weight = weights.Average() };
                    })
                    .ToDictionary(x=>x.Key, x=> x.Weight);

            switch (report.Reportoar.Calculation)
            {
                case CalculationType.Sample:
                    return 1;
                case CalculationType.Frequency:
                    return weightMap.Sum(x=>x.Value);
                case CalculationType.Volume:
                    return events.Select(x=> GetNbrOfMinutesInReportoire(time,report.Reportoar.Time, x) * weightMap[x.Individual.Id] ).Sum();
                case CalculationType.Custom:
                    return report.Reportoar.CalculationFunc(events);
                default:
                    throw new ArgumentException("");
            }
        }

        private Matrix AddZeroCells(Matrix res,Report report,List<Event> evs)
        {
            if(report.Columns == ReportDimension.Time && report.Reportoar.Time == PeriodType.Minute)
            {
                var start = TimeSpan.FromHours(evs.Min(x=>x.Timestamp).Hours);
                foreach(var row in res)
                {
                    foreach(var time in Enumerable.Range(0,60 * 24).Select(x=> start.Add(TimeSpan.FromMinutes(x))))
                    {
                        var name = DateTime.Today.ToString(time,PeriodType.Minute);
                        if(!row.Value.ContainsKey(name))
                        {
                            row.Value.Add(name,0);
                        }
                    }
                }
            }
            return res;
        }

        private Matrix TransformVolumePercentage(Matrix res)
        {
            var columnNames = res.SelectMany(x => x.Value.Select(y => y.Key)).Distinct().ToList();
            foreach (var columnName in columnNames)
            {
                var total = res.Sum(x => x.Value.ContainsKey(columnName) ? x.Value[columnName] : 0);
                foreach (var item in res)
                {
                    if (item.Value.ContainsKey(columnName))
                    {
                        item.Value[columnName] = (item.Value[columnName] / total) * 100.0;
                    }
                }
            }
            return res;
        }
        private Matrix TransformRank(Matrix res)
        {
            var columnNames = res.SelectMany(x => x.Value.Select(y => y.Key)).Distinct().ToList();
            foreach (var columnName in columnNames)
            {
                var rank = res.Where(x => x.Value.ContainsKey(columnName))
                              .Select(x => x.Value[columnName])
                              .OrderByDescending(x => x)
                              .Select((n, i) => new { Number = n, Index = i + 1 })
                              .GroupBy(x => x.Number)
                              .ToDictionary(x => x.Key, x => x.Min(y => y.Index));

                foreach (var item in res)
                {
                    if (item.Value.ContainsKey(columnName))
                    {
                        item.Value[columnName] = rank[item.Value[columnName]];
                    }
                }
            }
            return res;
        }
        private string CreateCsv(Matrix res)
        {
            var separator = "\t";
            var columnNames = res.SelectMany(x => x.Value.Select(y => y.Key)).Distinct().OrderBy(x => x).ToList();

            var header = "Name" + separator + string.Join(separator, columnNames);

            var lines = res.OrderBy(x => x.Key).Select(row => row.Key + separator + string.Join(separator, columnNames.Select(columnName =>
                 row.Value.ContainsKey(columnName) ? Math.Round( row.Value[columnName],0) : 0
             )));

            var csv = header + Environment.NewLine + string.Join(Environment.NewLine, lines);

            return csv;
        }
        private string PrittyPrint(Matrix res)
        {
            var columns = res.SelectMany(x => x.Value.Select(y => new { y.Key, Length = Math.Max(y.Key.Length, Math.Round(y.Value,0).ToString("N0").Length) } ))
                .GroupBy(x=>x.Key)
                .OrderBy(x => x.Key)
                .Select(x=> new
                {
                    x.Key,
                    Length = x.Max(y=>y.Length) + 2
                }).ToList();

            var nameLength = Math.Max(4, res.Max(x => x.Key.Length)) + 2; 

            var header = "Name".PadRight(nameLength, ' ') + string.Join("", columns.Select(x=>x.Key.PadLeft(x.Length, ' ')));

            var lines = res.OrderBy(x => x.Key).Select(row => row.Key.PadRight(nameLength, ' ')  + string.Join("", columns.Select(column =>
                 (row.Value.ContainsKey(column.Key) ? Math.Round(row.Value[column.Key], 0) : 0).ToString("N0").PadLeft(column.Length, ' ')
             )));

            var csv = header + Environment.NewLine + string.Join(Environment.NewLine, lines);

            return csv;
        }

        byte[] CreateExcel(Matrix res,Report report, string templateDir)
        {
            var useTemplate = !string.IsNullOrEmpty(report.Template);
            using (var ts = useTemplate ? new MemoryStream(File.ReadAllBytes(Path.Combine(templateDir, report.Template))) : new MemoryStream())
            using (var ms = new MemoryStream(100000))
            using (var package = useTemplate ? new ExcelPackage(ms, ts) : new ExcelPackage(ms))
            {
                var worksheet = package.Workbook.Worksheets.Where(x => x.Name == "DATA").FirstOrDefault() ??
                                package.Workbook.Worksheets.Add("DATA");
                var row = 1;

                //Header1
                var columnNames = res.SelectMany(x => x.Value.Select(y => y.Key)).Distinct().OrderBy(x => x).ToList();
                var col = 2;
                foreach (var columnName in columnNames)
                {
                    worksheet.Cells[row, col++].Value = columnName;
                }

                row++;


                foreach (var r in res.OrderBy(x => x.Key))
                {
                    worksheet.Cells[row, 1].Value = r.Key;
                    col = 2;
                    foreach (var columnName in columnNames)
                    {
                        worksheet.Cells[row, col++].Value = r.Value.ContainsKey(columnName) ? Math.Round( r.Value[columnName],0) : 0;
                    }


                    row++;
                }

                package.Save();
                return ms.ToArray();
            }
        }
        List<((string consumer, string consumed, string period, string time) key, Event @event)> ExpandOnRepertoire(Event e, Reportoire d)
        {
            var consumer = d.Consumer(e);
            var consumed = d.Consumed(e); 
            var period = e.Date.ToString(e.Timestamp, d.Period);
            var expandedTime = Enumerable
                .Range(0, Math.Max(1, e.Duration.GetNbr(d.Time)))
                .Select(n => e.Date.ToString(e.Timestamp.Add(d.Time.GetTimeSpan(n)), d.Time) )
                .ToList();
            
            switch(consumed)
            {
                case string cs :
                    return expandedTime.Select(time => ((consumer, cs, period, time), e)).ToList();
                case List<Tuple<string,Consumed>> cs :
                    return expandedTime.Select(time => ((consumer, cs.FirstOrDefault()?.Item1 ?? "nn", period, time), e)).ToList();
                default :
                    throw new ArgumentException("Can't use the consumer"); 
            }
        }
        (string row, string column) CalculateAggregationKey((string consumer, string consumed, string period, string time) c, Report report)
        {
            var rows = report.Rows == ReportDimension.Consumer ? c.consumer :
                       report.Rows == ReportDimension.Consumed ? c.consumed :
                       report.Rows == ReportDimension.Period ? c.period :
                       report.Rows == ReportDimension.Time ? c.time :
                       throw new ArgumentException();

            var cols = report.Columns == ReportDimension.Consumer ? c.consumer :
                       report.Columns == ReportDimension.Consumed ? c.consumed :
                       report.Columns == ReportDimension.Period ? c.period :
                       report.Columns == ReportDimension.Time ? c.time :
                       throw new ArgumentException();

            return (rows, cols);
        }
        
        public int GetNbrOfMinutesInReportoire(string time, PeriodType type, Event e)
        {
            switch (type)
            {
                case PeriodType.Minute:
                    return 1;
                case PeriodType.Hour:
                    return (int)e.Duration.TotalMinutes;
                case PeriodType.DayPart:
                    return (int)e.Duration.TotalMinutes;
                case PeriodType.Day:
                case PeriodType.Week:
                case PeriodType.Month:
                default:
                    return (int)e.Duration.TotalMinutes;
            }
            
        }

    }
    internal static class ReportExtension
    {
        public static string ToString(this DateTime date,TimeSpan time, PeriodType type)
        {
            switch (type)
            {
                case PeriodType.Minute:
                    return ((int)time.TotalHours * 60 + time.Minutes).ToString("0000");
                case PeriodType.Hour:
                    return time.TotalHours.ToString("00");
                case PeriodType.DayPart:
                    var partsLength = 3;
                    return (((int)time.TotalHours / partsLength) * partsLength).ToString("00") +"-" + ((((int)time.TotalHours + partsLength) / partsLength) * partsLength).ToString("00");
                case PeriodType.Day:
                    return date.ToString("MMdd");
                case PeriodType.Week:
                case PeriodType.Month:
                default:
                    return date.Month.ToString("00");
            }
        }
        public static int GetNbr(this TimeSpan ts, PeriodType type)
        {
            switch (type)
            {
                case PeriodType.Minute:
                    return (int)ts.TotalMinutes;
                case PeriodType.Hour:
                    return (int)ts.TotalHours;
                case PeriodType.DayPart:
                    return ((int)ts.TotalHours / 3);
                case PeriodType.Day:
                    return ts.Days;
                case PeriodType.Week:
                case PeriodType.Month:
                default:
                    return ts.Days / 30;
            }
        }
        public static TimeSpan GetTimeSpan(this PeriodType type, int unit)
        {
            switch (type)
            {
                case PeriodType.Minute:
                    return TimeSpan.FromMinutes(1 * unit);
                case PeriodType.Hour:
                    return TimeSpan.FromHours(1 * unit);
                case PeriodType.DayPart:
                    return TimeSpan.FromHours(3 * unit);
                case PeriodType.Day:
                    return TimeSpan.FromDays(1 * unit);
                case PeriodType.Week:
                case PeriodType.Month:
                default:
                    return TimeSpan.FromDays(30 * unit);
            }
        }
    }
}