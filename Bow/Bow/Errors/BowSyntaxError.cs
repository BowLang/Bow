namespace Errors;

[Serializable]
public class BowSyntaxError : Exception
{
    public BowSyntaxError() { }
    public BowSyntaxError(string message) : base(message) { }
    public BowSyntaxError(string message, Exception inner) : base(message, inner) { }
    
    protected BowSyntaxError(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
