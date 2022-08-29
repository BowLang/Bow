using Errors;
using Parse.Expressions.ObjInstances;

namespace Parse.Expressions;

public class BuiltinObjectExpression : Expression
{
    private readonly string _type;
    private readonly string _value;
    private readonly int _line;
    
    public BuiltinObjectExpression(string type, string value, int line)
    {
        _type = type;
        _value = value;
        _line = line;
    }
    
    public override ObjInstance Evaluate()
    {
        return _type switch
        {
            "str" => new StrInstance(_value, _line),
            "dec" => new DecInstance(_value, _line),
            "boo" => new BooInstance(_value, _line),
            _ => throw new BowRuntimeError($"Unknown builtin type on line {_line}")
        };
    }
}
