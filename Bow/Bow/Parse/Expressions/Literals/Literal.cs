using Tokenise;
namespace Parse.Expressions.Literals;

public class Literal
{
    public dynamic Value { get; init; } = "";
    public string Type { get; init; } = "";

    public string DisplayValue
    {
        get
        {
            if (Type == TokenType.StrLiteral)
            {
                return $"\"{Value}\"";
            }
            
            return Value.ToString();
        }
    } 
}
