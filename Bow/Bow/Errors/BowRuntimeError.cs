namespace Errors;

[Serializable]
public class BowRuntimeError : Exception
{
    public BowRuntimeError() : base() { }
    public BowRuntimeError(string message) : base(message) { }
    public BowRuntimeError(string message, Exception inner) : base(message, inner) { }
    
    protected BowRuntimeError(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
