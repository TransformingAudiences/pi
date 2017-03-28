using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;

namespace tapi
{
    public class Unit : DynamicObject
    {

  
        
        public ImmutableDictionary<string, VariableValue> Variables { get; private set; }
      
        protected Unit(IEnumerable<VariableValue> attributes)
        {
            Variables = attributes.ToImmutableDictionary(x => x.Field.Name);
        }
        protected Unit(ImmutableDictionary<string, VariableValue> attributes)
        {
            Variables = attributes;
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
                throw new ArgumentException("binder can't be null");

            var name = binder.Name;
            if (Variables.ContainsKey(name))
            {
                result = Variables[name].Value;
            }
            else
            {
                result = null;
            }

            return true;
        }

        public override string ToString()
        {
            return base.ToString();
        }
       

        public string GetVariableDisplayValue(string attributeName)
        {
            return this.Variables.ContainsKey(attributeName) ?
                       this.Variables[attributeName].DisplayValue :
                       null;

        }
      
        public bool HasVariable(string name)
        {
            return Variables.ContainsKey(name);
        }
        public bool HasNotVariable(string name)
        {
            return !Variables.ContainsKey(name);
        }
        /// <summary>
        public T GetVariableValue<T>(string attributeName, T defaultValue = default(T))
        {
            return this != null ?
                       this.Variables.ContainsKey(attributeName) ?
                             this.Variables[attributeName].Value is T ?
                             (T)this.Variables[attributeName].Value :

                       defaultValue :
                   defaultValue : defaultValue;

        }
      
    }
}