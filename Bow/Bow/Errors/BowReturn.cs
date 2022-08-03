using Parse.Expressions.Literals;

namespace Errors;

[Serializable]
public class BowReturn : Exception
{
    public Literal? Literal { get; } 
    
    public BowReturn() : base() { }
    public BowReturn(string message) : base(message) { }
    public BowReturn(string message, Exception inner) : base(message, inner) { }

    public BowReturn(Literal? literal) : base()
    {
        Literal = literal;
        
    }
    
    protected BowReturn(System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
