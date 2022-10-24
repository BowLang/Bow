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
        if (Env.IsFunctionDefined(_name, _line))
        {
            return new FunctionExpression(_name, new List<Expression>(), _line).Evaluate();
        }

        if (Env.IsObjectDefined(_name, _line))
        {
            return Env.GetObject(_name, _line).Static;
        }

        return Env.IsMethodDefined(_name)
            ? Env.CurrentInstanceObj!.ExecuteMethod(_name, new List<Expression>(), false, _line)
            : Env.GetVariable(_name, _line).Object;
    }
}
