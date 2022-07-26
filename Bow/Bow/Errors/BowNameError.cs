namespace Errors;

[Serializable]
public class BowNameError : Exception
{
    public BowNameError() : base() { }
    public BowNameError(string message) : base(message) { }
    public BowNameError(string message, Exception inner) : base(message, inner) { }
    
    protected BowNameError(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}