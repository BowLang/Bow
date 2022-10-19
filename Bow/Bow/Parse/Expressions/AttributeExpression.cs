using Errors;
using Parse.Environment;
using Parse.Expressions.Objects;

namespace Parse.Expressions;

public class AttributeExpression : Expression
{
    private readonly string _name;
    private readonly Expression? _target;
    private readonly int _line;
    
    public AttributeExpression(string name, Expression? target, int line)
    {
        _name = name;
        _target = target;
        _line = line;
    }

    public override Obj Evaluate()
    {
        /*ObjInstance target;
        
        if (_target is null)
        {
            target = Env.CurrentInstanceObj ??
                     throw new BowSyntaxError(
                         $"Cannot access attribute '{_name}' outside of an object on line {_line}");
        }
        else
        {
            target = _target.Evaluate();
        }*/
        
        Obj target = _target is null
            ? Env.CurrentInstanceObj ??
              throw new BowSyntaxError($"Cannot access attribute '{_name}' outside of an object on line {_line}")
            : _target.Evaluate();

        Obj? startObj = Env.CurrentInstanceObj;
        Env.CurrentInstanceObj = target;

        Obj result = target.GetAttribute(_name, _line).Object;
        
        Env.CurrentInstanceObj = startObj;

        return result;
    }
}
