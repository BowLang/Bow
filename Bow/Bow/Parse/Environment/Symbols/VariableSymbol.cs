using Parse.Expressions.Objects;

namespace Parse.Environment;

public class VariableSymbol
{
    public string Name { get; }
    public Obj Object { get; private set; }
    public int Line { get; }
    public bool IsConstant { get; }

    public VariableSymbol(string name, Obj obj, int line, bool isConstant)
    {
        Name = name;
        Object = obj;
        Line = line;
        IsConstant = isConstant;
    }

    public void SetValue(Obj newValue)
    {
        Object = newValue;
    }
}
