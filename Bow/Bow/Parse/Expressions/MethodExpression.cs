using Parse.Expressions.Objects;

namespace Parse.Expressions;

public class MethodExpression : Expression
{
    private readonly string _methodName;
    private readonly List<Expression> _arguments;
    private readonly Expression _target;
    private readonly int _line;
    
    public MethodExpression(string methodName, List<Expression> arguments, Expression target, int line)
    {
        _methodName = methodName;
        _arguments = arguments;
        _target = target;
        _line = line;
    }
    
    public override Obj Evaluate()
    {
        Obj target = _target.Evaluate();
        
        return target.ExecuteMethod(_methodName, _arguments, true, _line);
    }
}
