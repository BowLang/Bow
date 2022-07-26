using Errors;

namespace Parse.Environment;

public class Symbol
{
    public string Name { get;  }
    public string Type { get; }
    public string Value { get; }
    public int Line { get; }
    public bool IsConstant { get; set;  }

    public Symbol(string name, string type, string value, int line, bool isConstant)
    {
        Name = name;
        Type = type;
        Value = value;
        Line = line;
        IsConstant = isConstant;
    }
}
