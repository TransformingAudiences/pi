using System;
using System.Collections.Generic;
using System.Linq;

namespace tapi
{
    public class DayConsumer : Unit
    {
        public string Id { get; }
        public double Weight { get; }
        public DateTime Date { get; }
        public DayConsumer(string id, DateTime date,double weight, IEnumerable<VariableValue> variables): base(variables)
        {
            Id = id;
            Weight = weight;
            Date = date;
        }

        public static Consumer Merge(IEnumerable<DayConsumer> consumers)
        {
            return new Consumer(
                consumers.First().Id,
                consumers.ToDictionary(x=>x.Date,x=>x.Weight),
                consumers.First().Variables.Values);
        }
    }
}