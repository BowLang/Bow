using Errors;
using Parse.Expressions;
using Parse.Expressions.Literals;


namespace Parse.Statements;

public class Return : Statement
{
    private readonly Expression? _expression;
    
    public Return(Expression? expression)
    {
        _expression = expression;
    }

    public override void Interpret()
    {
        Literal? literal = null;
        if (_expression is not null)
        {
            literal = _expression.Evaluate();
        }

        throw new BowReturn(literal);
    }
}