using Errors;
using Parse.Expressions;
using Parse.Environment;
using Parse.Expressions.Literals;
using Tokenise;

namespace Parse.Statements;

public class If : Statement
{
    private readonly Expression _ifCondition;
    private readonly List<Statement> _ifStatements;
    private readonly List<Tuple<Expression, List<Statement>>> _altIfs;
    private readonly List<Statement> _altStatements;
    private readonly int _line;
    
    public If(Expression ifCondition, List<Statement> ifStatements, List<Tuple<Expression, List<Statement>>> altIfs,
        List<Statement> altStatements, int line)
    {
        _ifCondition = ifCondition;
        _ifStatements = ifStatements;
        _altIfs = altIfs;
        _altStatements = altStatements;
        _line = line;
    }

    public override void Interpret()
    {
        Literal condition = _ifCondition.Evaluate();

        if (condition.Type != TokenType.BooLiteral)
        {
            throw new BowTypeError($"If condition must be a boolean, but was {condition.Type} on line {_line}");
        }
        
        // If statement

        if (condition.Value)
        {
            InterpretBranch(_ifStatements);
            return;
        }
        
        // Alt if statements
        
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
                InterpretBranch(altIfStatements);
                return;
            }
        }
        
        // Alt statement
        
        InterpretBranch(_altStatements);
    }

    private void InterpretBranch(List<Statement> statements)
    {
        Env.PushScope(new Env());

        foreach (Statement statement in statements)
        {
            statement.Interpret();
        }

        Env.PopScope();
    }
}
