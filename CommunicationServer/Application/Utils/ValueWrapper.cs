namespace Application.Utils;

/*
 * This class is used when we want to pass a primitive type by reference
 */
public class ValueWrapper<T> where T : struct
{
    public T Value { get; set; }

    public ValueWrapper(T value)
    {
        Value = value;
    }
}
