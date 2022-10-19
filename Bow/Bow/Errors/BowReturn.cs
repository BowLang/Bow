using Parse.Expressions.Objects;

namespace Errors;

[Serializable]
public class BowReturn : Exception
{
    public Obj Object { get; }

    public BowReturn(Obj obj)
    {
        Object = obj;
    }
}
