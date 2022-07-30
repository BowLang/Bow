namespace Errors;

[Serializable]
public class BowEOFError : Exception
{
    public BowEOFError() : base() { }
    public BowEOFError(string message) : base(message) { }
    public BowEOFError(string message, Exception inner) : base(message, inner) { }
    
    protected BowEOFError(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
