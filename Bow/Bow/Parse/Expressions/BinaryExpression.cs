using Parse.Expressions.Objects;

namespace Parse.Expressions;

public class BinaryExpression : Expression
{
    private readonly Expression _left;
    private readonly Expression _right;
    private readonly string _operator;
    private readonly int _line;
    
    public BinaryExpression(Expression left, string op, Expression right, int line)
    {
        _left = left;
        _right = right;
        _operator = op;
        _line = line;
    }

    public override Obj Evaluate()
    {
        return _left.Evaluate().ExecuteMethod(_operator, new List<Expression> { _right }, true, _line);
    } 
}
