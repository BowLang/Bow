using Tokenise;

namespace Parse.Expressions.Literals;

public class BooLiteral : Literal
{
    public BooLiteral(string value)
    {
        Value = bool.Parse(value);
        Type = TokenType.BooLiteral;
    }

    public BooLiteral(bool value)
    {
        Value = value;
        Type = TokenType.BooLiteral;
    }
}
