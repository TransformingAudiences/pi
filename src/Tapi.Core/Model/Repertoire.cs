using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace tapi
{
    public class Repertoire
    {
        public string Name {get;}
        public Func<Event,string> Consumer { get;  }
        public Func<Event,object> Consumed { get;  }
        public PeriodType Period { get; }
        public PeriodType Time { get;  }
        public CalculationType Calculation {get;}
        public Func<IEnumerable<Event>, double> CalculationFunc { get;  }
        public Func<Event, bool> Filter { get;  }
        private Repertoire(string name, Func<Event,string> consumer, Func<Event,object> consumed,PeriodType period,PeriodType time, CalculationType calculation,Func<IEnumerable<Event>, double> calculationScript, Func<Event,bool> filter)
        {
            Name = name;
            Consumer = consumer;
            Consumed = consumed;
            Period = period;
            Time = time;
            Calculation = calculation;
            CalculationFunc = calculationScript; 
            Filter = filter;
        }
        public static Repertoire Load(XElement xml)
        {
            var name = xml.Attribute("name")?.Value as string;
            var consumer = xml.Attribute("consumer")?.Value as string;
            var consumed = xml.Attribute("consumed")?.Value as string;
            var periodIn = xml.Attribute("period")?.Value as string ?? PeriodType.Day.ToString();
            var timeIn = xml.Attribute("time")?.Value as string ?? PeriodType.Minute.ToString();
            var calculationIn = xml.Attribute("calculation")?.Value as string;
            var calculationScript = xml.Attribute("calculationScript")?.Value as string;
            var filter = xml.Attribute("filter")?.Value as string;
            var consumedSource = xml.Attribute("consumedSource")?.Value as string;
            
            var calculation = (CalculationType)Enum.Parse(typeof(CalculationType),calculationIn);
            var consumerFunc =  Script.Compile<Func<Event,string>>($"var E = e as dynamic; return E.{consumer}.ToString();",new string[]{"e"});
            var consumedFunc = !consumed.StartsWith("Product.") 
                ? Script.Compile<Func<Event,object>>($"var E = e as dynamic; return E.{consumed}.ToString();",new string[]{"e"})
                : Script.Compile<Func<Event,object>>(
                    $@"var cs = e.Products.ContainsKey(""{consumedSource}"") ? e.Products[""{consumedSource}""] : new List<Consumed>(); 
                    return cs.Select(x=>
                    {{
                        var E = x as dynamic; 
                        var name = E.{consumed.Replace("Product.","")}.ToString() as string;
                        return (name,x);
                    }}).ToList();",new string[]{"e"});
            
            var calculationFunc = calculation == CalculationType.Custom
                ? Script.Compile<Func<IEnumerable<Event>,double>>($"return (double)events.{calculationScript};",new string[] {"events"})
                : null;
            var filterFunc = Script.Compile<Func<Event,bool>>(
                filter == null
                  ? "return true;"
                  : $"var E = e as dynamic; return E.{filter};",new string[] {"e"});
            
            var period = (PeriodType)Enum.Parse(typeof(PeriodType), periodIn);
            var time = (PeriodType)Enum.Parse(typeof(PeriodType), timeIn);

            return new Repertoire(name, consumerFunc, consumedFunc, period, time, calculation,calculationFunc, filterFunc);
        }
    }

    public enum ReportDimension
    {
        Consumer,
        Consumed,
        Period,
        Time
    }
    public enum PeriodType
    {
        Minute,
        Hour,
        DayPart,
        Day,
        Week,
        Month
    }
    public enum AggregateType
    {
        Count,
        Sum,
        Average
    }
    public enum PostProcessType
    {
        None,
        VolumePercentage,
        Rank
    }
    public enum OutputFormat
    {
        Xlsx,
        Csv
    }
}