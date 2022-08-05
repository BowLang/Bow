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

        try
        {
            foreach (var caseTuple in from caseTuple in _cases
                     from caseExpression in caseTuple.Item1
                     let caseLiteral = caseExpression.Evaluate()
                     where caseLiteral.Value == switchExpression.Value
                     select caseTuple)
            {
                InterpretBranch(caseTuple.Item2);
                return;
            }
        }
        catch (RuntimeBinderException)
        {
            throw new BowTypeError($"Cannot compare different types in switch statement on line {_line}");
        }

        InterpretBranch(_other);
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
