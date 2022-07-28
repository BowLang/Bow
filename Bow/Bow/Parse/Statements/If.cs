using Errors;
using Parse.Expressions;
using Parse.Environment;
using Parse.Expressions.Literals;
using Tokenise;

namespace Parse.Statements;

public class If : Statement
{
    private readonly Expression _condition;
    private readonly List<Statement> _statements;
    private readonly int _line;
    
    public If(Expression condition, List<Statement> statements, int line)
    {
        _condition = condition;
        _statements = statements;
        _line = line;
    }

    public override void Interpret()
    {
        Literal condition = _condition.Evaluate();

        if (condition.Type != TokenType.BooLiteral)
        {
            throw new BowTypeError($"If  condition must be a boolean, but was {condition.Type} on line {_line}");
        }
        
        if (condition.Value)
        {
            Env.PushScope(new Env());
            
            foreach (Statement statement in _statements)
            {
                statement.Interpret();
            }
            
            Env.PopScope();
        }
    }
}
