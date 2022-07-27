using Parse.Expressions.Literals;

namespace Parse.Environment;

public class Symbol
{
    public string Name { get;  }
    public Literal Literal { get;  }
    public int Line { get; }
    public bool IsConstant { get;  }

    public Symbol(string name, Literal literal, int line, bool isConstant)
    {
        Name = name;
        Literal = literal;
        Line = line;
        IsConstant = isConstant;
    }
}
