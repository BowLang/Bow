using Errors;
using Parse.Expressions;
using Parse.Environment;
using Parse.Expressions.Literals;
using Tokenise;

namespace Parse.Statements;

public class Alt : Statement
{
    private readonly Expression _condition;
    private readonly List<Statement> _ifStatements;
    private readonly List<Statement> _altStatements;
    private readonly int _line;
    
    public Alt(Expression condition, List<Statement> ifStatements, List<Statement> altStatements, int line)
    {
        _condition = condition;
        _ifStatements = ifStatements;
        _altStatements = altStatements;
        _line = line;
    }

    public override void Interpret()
    {
        Literal condition = _condition.Evaluate();

        if (condition.Type != TokenType.BooLiteral)
        {
            throw new BowTypeError($"If  condition must be a boolean, but was {condition.Type} on line {_line}");
        }
        
        Env.PushScope(new Env());
        
        foreach (Statement statement in condition.Value ? _ifStatements : _altStatements)
        {
            statement.Interpret();
        }
        
        Env.PopScope();
    }
}
