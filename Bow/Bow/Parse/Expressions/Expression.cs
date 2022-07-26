using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class Expression
{
    public virtual Literal Evaluate()
    {
        throw new NotImplementedException();
    }
}
