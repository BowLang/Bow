using Parse.Expressions.ObjInstances;

namespace Parse.Expressions;

public class UnaryExpression : Expression
{
    private readonly Expression _right;
    private readonly string _operator;
    private readonly int _line;
    
    public UnaryExpression(string op, Expression right, int line)
    {
        _right = right;
        _operator = op;
        _line = line;
    }

    public override ObjInstance Evaluate()
    {
        return _right.Evaluate().ExecuteMethod(_operator, new List<Expression>(), _line);
    }
}
