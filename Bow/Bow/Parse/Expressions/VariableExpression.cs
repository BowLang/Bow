using Parse.Environment;
using Parse.Expressions.Objects;

namespace Parse.Expressions;

public class VariableExpression : Expression
{
    private readonly string _name;
    private readonly int _line;
    
    public VariableExpression(string name, int line)
    {
        _name = name;
        _line = line;
    }

    public override Obj Evaluate()
    {
        return Env.IsFunctionDefined(_name, _line)
            ? new FunctionExpression(_name, new(), _line).Evaluate()
            : Env.IsObjectDefined(_name, _line) ? Env.GetObject(_name, _line).Static : Env.GetVariable(_name).Object;
    }
}
