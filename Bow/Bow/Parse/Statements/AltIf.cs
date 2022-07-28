using System.Diagnostics;
using Errors;
using Parse.Expressions;
using Parse.Environment;
using Parse.Expressions.Literals;
using Tokenise;

namespace Parse.Statements;

public class AltIf : Statement
{
    private readonly Expression _condition;
    private readonly List<Statement> _ifStatements;
    private readonly List<Tuple<Expression, List<Statement>>> _altIfs;
    private readonly List<Statement> _altStatements = new();
    private readonly int _line;
    
    public AltIf(Expression condition, List<Statement> ifStatements, List<Tuple<Expression, List<Statement>>> altIfs,
        List<Statement> altStatements, int line)
    {
        _condition = condition;
        _ifStatements = ifStatements;
        _altIfs = altIfs;
        _altStatements = altStatements;
        _line = line;
    }
    
    public AltIf(Expression condition, List<Statement> ifStatements, List<Tuple<Expression, List<Statement>>> altIfs, int line)
    {
        _condition = condition;
        _ifStatements = ifStatements;
        _altIfs = altIfs;
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

            foreach (Statement statement in _ifStatements)
            {
                statement.Interpret();
            }

            Env.PopScope();

            return;
        }
        
        foreach (var (altIfCondition, altIfStatements) in _altIfs)
        {
            Literal evalledAltIfCondition = altIfCondition.Evaluate();

            if (evalledAltIfCondition.Type != TokenType.BooLiteral)
            {
                throw new BowTypeError(
                    $"AltIf condition must be a boolean, but was {evalledAltIfCondition.Type} on line {_line}");
            }

            if (evalledAltIfCondition.Value)
            {
                Env.PushScope(new Env());

                foreach (Statement statement in altIfStatements)
                {
                    statement.Interpret();
                }

                Env.PopScope();

                return;
            }
        }
        
        Env.PushScope(new Env());
        
        foreach (Statement statement in _altStatements)
        {
            statement.Interpret();
        }
        
        Env.PopScope();
    }
}
