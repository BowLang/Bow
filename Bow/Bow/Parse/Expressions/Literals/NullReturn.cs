using Tokenise;

namespace Parse.Expressions.Literals;

public class NullReturn : Literal
{
    public NullReturn()
    {
        Value = "null";
        Type = TokenType.NullReturn;
    }
}
