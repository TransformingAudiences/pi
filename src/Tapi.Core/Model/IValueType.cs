
namespace tapi
{
    public interface IValueType
    {
        object Deserialize(string value);
        string ToDisplayText(object value);
        bool EqualTo(object first,object second);
    }
}