using Tokenise;

namespace Parse.Expressions.Literals;

public class DecLiteral : Literal
{
    public DecLiteral(string value)
    {
        Value = double.Parse(value);
        Type = TokenType.DecLiteral;
    }

    public DecLiteral(double value)
    {
        Value = value;
        Type = TokenType.DecLiteral;
    }
}
