using Parse.Expressions.ObjInstances;

namespace Parse.Environment;

public class AttributeSymbol
{
    public string Name { get; }
    public ObjInstance Object { get; private set; }
    public ObjectSymbol Type { get; init; }
    public int Line { get; }
    public bool IsConstant { get; }
    public bool IsPrivate { get; }

    public AttributeSymbol(string name, ObjectSymbol type, int line, bool isConstant, bool isPrivate)
    {
        Name = name;
        Type = type;
        Object = new NullInstance();
        Line = line;
        IsConstant = isConstant;
        IsPrivate = isPrivate;
    }

    public void SetValue(ObjInstance newObj)
    {
        Object = newObj;
    }

    public AttributeSymbol Copy()
    {
        AttributeSymbol newAttr = new AttributeSymbol(Name, Type, Line, IsConstant, IsPrivate);
        newAttr.SetValue(Object);

        return newAttr;
    }
}
