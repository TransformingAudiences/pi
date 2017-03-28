using System;

namespace tapi
{
     public class VariableValue
    {
        public Field Field { get; private set; }
        public virtual object Value { get; private set; }

        public VariableValue( Field attribute, string value)
        {
            Field = attribute;
            try
            {
                Value = Field.ValueType.Deserialize(value);
            }
            catch
            {
                throw;
            }
        }
        public string DisplayValue
        {
            get
            {
                try
                {
                    return Field.ValueType.ToDisplayText(Value);
                }
                catch (Exception ex)
                {
                    return "ERROR: " + ex.GetMessage();
                }


            }
        }
        public bool EqualTo(object value)
        {
            return Field.ValueType.EqualTo(Value, value);
        }
        public override string ToString()
        {
            return Value != null ? Value.ToString() : "";
        }
    }
}