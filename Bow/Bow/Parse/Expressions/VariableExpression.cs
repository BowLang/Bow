using Parse.Environment;
using Parse.Expressions.ObjInstances;

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

    public override ObjInstance Evaluate()
    {
        return Env.IsFunctionDefined(_name, _line)
            ? new FunctionExpression(_name, new(), _line).Evaluate()
            : Env.GetVariable(_name).Object;
    }
}
