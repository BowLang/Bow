using Errors;
using Parse.Expressions;
using Parse.Environment;
using Parse.Expressions.Literals;

using Microsoft.CSharp.RuntimeBinder;

namespace Parse.Statements;

public class Switch : Statement
{
    private readonly Expression _switchExpression;
    private readonly List<Tuple<List<Expression>, List<Statement>>> _cases;
    private readonly List<Statement> _other;
    private readonly int _line;
    
    public Switch(Expression switchExpression, List<Tuple<List<Expression>, List<Statement>>> cases, List<Statement> other, int line)
    {
        _switchExpression = switchExpression;
        _cases = cases;
        _other = other;
        _line = line;
    }

    public override void Interpret()
    {
        Literal switchExpression = _switchExpression.Evaluate();

        int count = 0;
        bool match = false;
        bool escaped = false;
        while (count < _cases.Count)
        {
            var (comparisons, statements) = _cases[count];
            
            foreach (Expression comparison in comparisons)
            {
                Literal comparisonLiteral = comparison.Evaluate();
                if (comparisonLiteral.Type != switchExpression.Type)
                {
                    throw new BowTypeError($"Cannot compare {comparisonLiteral.Type} with {switchExpression.Type} on line {_line}");
                }
                
                if (match || comparisonLiteral.Value == switchExpression.Value)
                {
                    try
                    {
                        InterpretBranch(statements);
                    }
                    catch (BowBreak)
                    {
                        escaped = true;
                    }

                    match = true;
                    break;
                }
            }

            if (escaped) break;
            
            count++;
        }

        if (!escaped)
        {
            try
            {
                InterpretBranch(_other);
            }
            catch (BowBreak) { }
        }
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
