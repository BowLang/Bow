using Errors;
using Parse.Expressions;
using Parse.Environment;
using Parse.Expressions.Objects;

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
        Obj condition = _ifCondition.Evaluate();

        if (!new BooInstance(true, _line).AcceptsType(condition))
        {
            throw new BowTypeError(
                $"If condition must be a boolean, but was {condition.DisplayName()} on line {_line}");
        }

        BooInstance boo = (BooInstance)condition;
        
        // If statement

        if (boo.Value)
        {
            InterpretBranch(_ifStatements);
            return;
        }
        
        // Alt if statements
        
        foreach (var (altIfCondition, altIfStatements) in _altIfs)
        {
            Obj evalledAltIfCondition = altIfCondition.Evaluate();

            if (!new BooInstance(true, _line).AcceptsType(evalledAltIfCondition))
            {
                throw new BowTypeError(
                    $"AltIf condition must be a boolean, but was {evalledAltIfCondition.DisplayName()} on line {_line}");
            }
            
            BooInstance altIfBoo = (BooInstance)evalledAltIfCondition;

            if (!altIfBoo.Value) continue;
            
            InterpretBranch(altIfStatements);
            return;
        }
        
        // Alt statement
        
        InterpretBranch(_altStatements);
    }

    private static void InterpretBranch(List<Statement> statements)
    {
        Env.PushScope(new Env());

        foreach (Statement statement in statements)
        {
            statement.Interpret();
        }

        Env.PopScope();
    }
}
