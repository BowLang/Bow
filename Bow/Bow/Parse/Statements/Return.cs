using Errors;
using Parse.Expressions;
using Parse.Expressions.ObjInstances;


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
        ObjInstance returnedValue = new NullInstance();
        if (_expression is not null)
        {
            returnedValue = _expression.Evaluate();
        }

        throw new BowReturn(returnedValue);
    }
}
