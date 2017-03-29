using System;
using System.Collections.Generic;
using System.Linq;

namespace tapi
{
    public class Event : Unit
    {
        public DateTime Date {get;}
        public Consumer Individual { get;  }
        public Dictionary<string,List<Consumed>> Products { get; }
        public TimeSpan StartTime { get; }
        public TimeSpan EndTime => StartTime.Add(Duration);
        public TimeSpan Duration {get;}
        public Event(DateTime date, TimeSpan timestamp, Consumer consumer, List<Consumed> consumed, IEnumerable<VariableValue> variables): base(variables)
        {
            Date = date;
            StartTime = timestamp;
            Individual = consumer;
            Products = consumed.GroupBy(x=>x.Source).ToDictionary(x=>x.Key, x=>x.ToList());

            var st = this.GetVariableValue<TimeSpan>("StartTime");
            var et = this.GetVariableValue<TimeSpan>("EndTime");
            Duration = et - st;
        }
    }
}