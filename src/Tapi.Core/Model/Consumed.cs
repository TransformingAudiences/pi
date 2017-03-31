using System;
using System.Collections.Generic;

namespace tapi
{
    public class Consumed : Unit
    {
        public int Id { get; }
        public DateTime Date { get; }
        public TimeSpan StartTime {get;}
        public TimeSpan EndTime {get;} 
        public string Source {get;}
        public (TimeSpan start, TimeSpan end) Interval => (StartTime, EndTime);
        public Consumed(int id,DateTime date, TimeSpan startTime,int duration,string source, IEnumerable<VariableValue> variables): base(variables)
        {
            Id = id;
            Date = date;
            StartTime = startTime;
            EndTime =startTime.Add(TimeSpan.FromSeconds( duration));
            Source = source;
        }
      
    }
}