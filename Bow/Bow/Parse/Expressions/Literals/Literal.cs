namespace Parse.Expressions.Literals;

public class Literal
{
    public string Value { get; }
    public string Type { get; }
    
    public Literal(string value, string type)
    {
        Value = value;
        Type = type;
    }
}
