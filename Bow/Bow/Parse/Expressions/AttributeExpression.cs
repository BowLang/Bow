using Errors;
using Parse.Environment;
using Parse.Expressions.Objects;

namespace Parse.Expressions;

public class AttributeExpression : Expression
{
    private readonly string _name;
    public readonly Expression? Target;
    private readonly int _line;
    
    public AttributeExpression(string name, Expression? target, int line)
    {
        _name = name;
        Target = target;
        _line = line;
    }

    public override Obj Evaluate()
    {
        Obj target = Target is null
            ? Env.CurrentInstanceObj ??
              throw new BowSyntaxError($"Cannot access attribute '{_name}' outside of an object on line {_line}")
            : Target.Evaluate();

        Obj? startObj = Env.CurrentInstanceObj;
        Env.CurrentInstanceObj = target;

        Obj result = target.GetAttribute(_name, Target is not null, _line).Object;
        
        Env.CurrentInstanceObj = startObj;

        return result;
    }
}
