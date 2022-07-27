using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class LiteralExpression : Expression
{
    private readonly Literal _literal;
    private readonly int _line;
    
    public LiteralExpression(Literal literal, int line)
    {
        _literal = literal;
        _line = line;
    }

    public override Literal Evaluate()
    {
        return _literal;
    }
}
