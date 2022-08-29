namespace Errors;

[Serializable]
public class BowTypeError : Exception
{
    public BowTypeError() { }
    public BowTypeError(string message) : base(message) { }
    public BowTypeError(string message, Exception inner) : base(message, inner) { }
    
    protected BowTypeError(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
