using Parse.Expressions.Literals;

namespace Parse.Environment;

public class VariableSymbol
{
    public string Name { get; }
    public Literal Literal { get; private set; }
    public int Line { get; }
    public bool IsConstant { get; }

    public VariableSymbol(string name, Literal literal, int line, bool isConstant)
    {
        Name = name;
        Literal = literal;
        Line = line;
        IsConstant = isConstant;
    }

    public void SetValue(Literal newValue)
    {
        Literal = newValue;
    }
}
