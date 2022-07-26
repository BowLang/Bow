using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class LiteralExpression : Expression
{
    private Literal _value;
    private int _line;
    
    public LiteralExpression(Literal value, int line)
    {
        _value = value;
        _line = line;
    }

    public override Literal Evaluate()
    {
        return _value;
    }
}
