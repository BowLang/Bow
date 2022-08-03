using Tokenise;
using Parse.Expressions;
using Parse.Expressions.Literals;

namespace Parse.Statements;

public class LitStatement : Statement
{
    private readonly Expression _valueExpression;
    private readonly int _line;
    
    public LitStatement(Expression valueExpression, int line)
    {
        _valueExpression = valueExpression;
        _line  = line;
    }

    public override void Interpret()
    {
        _valueExpression.Evaluate();
    }
    
    public override string Interpret(bool lastInShell)
    {
        return lastInShell ? _valueExpression.Evaluate().DisplayValue : "";
    }
}
