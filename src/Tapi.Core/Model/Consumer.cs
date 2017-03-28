using System;
using System.Collections.Generic;

namespace tapi
{
    public class Consumer : Unit
    {
        public string Id { get; }
        public Dictionary<DateTime, double> Weights { get; }
        public Consumer(string id, Dictionary<DateTime, double> weights, IEnumerable<VariableValue> variables) : base(variables)
        {
            Id = id;
            Weights = weights;
        }
    }
}