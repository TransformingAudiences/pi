using System;
using System.Collections.Generic;

namespace tapi
{
    public class Consumed : Unit
    {
        public int Id { get; }
        public DateTime StartTime {get;}
        public DateTime EndTime {get;} 
        public string Source {get;}
        public Consumed(int id, DateTime startTime,int duration,string source, IEnumerable<VariableValue> variables): base(variables)
        {
            Id = id;
            StartTime = startTime;
            EndTime =startTime.AddSeconds(duration);
            Source = source;
        }
        public bool IsOverlapping(DateTime start, DateTime end)
        {
            return this.StartTime <= start && this.EndTime >= end; 
        }
    }
}