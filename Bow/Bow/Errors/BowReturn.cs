using Parse.Expressions.ObjInstances;

namespace Errors;

[Serializable]
public class BowReturn : Exception
{
    public ObjInstance Object { get; }

    public BowReturn(ObjInstance obj)
    {
        Object = obj;
    }
}
