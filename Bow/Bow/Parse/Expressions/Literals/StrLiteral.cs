using Tokenise;

namespace Parse.Expressions.Literals;

public class StrLiteral : Literal
{
    public StrLiteral(string value)
    {
        Value = value;
        Type = TokenType.StrLiteral;
    }
}
