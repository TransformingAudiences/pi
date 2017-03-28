using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;


namespace tapi
{
    public sealed class MultipleOption
    {
        public ImmutableList<OptionValue> Values { get; private set; }
        public MultipleOption(IEnumerable<OptionValue> values)
        {
            Values = values.ToImmutableList();
        }
        public bool Contains(string name)
        {
            return Values.Any(x => x.Name == name);
        }
    }
}