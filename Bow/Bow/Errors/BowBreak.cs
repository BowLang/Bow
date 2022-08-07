namespace Errors;

[Serializable]
public class BowBreak : Exception
{
    public BowBreak() : base() { }
    public BowBreak(string message) : base(message) { }
    public BowBreak(string message, Exception inner) : base(message, inner) { }
    
    protected BowBreak(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
