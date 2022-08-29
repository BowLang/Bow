using Parse.Expressions.ObjInstances;

namespace Parse.Environment;

public class VariableSymbol
{
    public string Name { get; }
    public ObjInstance Object { get; private set; }
    public int Line { get; }
    public bool IsConstant { get; }

    public VariableSymbol(string name, ObjInstance obj, int line, bool isConstant)
    {
        Name = name;
        Object = obj;
        Line = line;
        IsConstant = isConstant;
    }

    public void SetValue(ObjInstance newValue)
    {
        Object = newValue;
    }
}
