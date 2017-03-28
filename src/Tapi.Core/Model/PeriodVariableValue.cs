using System;

namespace tapi
{
    public class PeriodVariableValue : VariableValue
    {
        public DateTime From { get; }
        public PeriodVariableValue(Field field, string value, DateTime from) : base(field,value)
        {
            From = from;
        }
    }
}