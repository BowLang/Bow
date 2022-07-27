namespace Parse.Expressions.Literals;

public class Literal
{
    public dynamic Value { get; init; } = "";
    public string Type { get; init; } = "";

    public string StrValue => Value.ToString();
}
