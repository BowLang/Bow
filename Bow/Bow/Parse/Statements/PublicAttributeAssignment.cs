using Errors;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.Objects;

namespace Parse.Statements;

public class PublicAttributeAssignment : Statement
{
    private readonly AttributeExpression _expression;
    private readonly string _name;
    private readonly Expression _newValue;
    private readonly int _line;

    public PublicAttributeAssignment(AttributeExpression expression, string name, Expression newValue, int line)
    {
        _expression = expression;
        _name = name;
        _newValue = newValue;
        _line = line;
    }

    public override void Interpret()
    {
        Expression? target = _expression.Target;

        if (target is null)
        {
            throw new BowRuntimeError($"Target is null in public attribute assignment at line {_line}");
        }

        Obj targetObj = target.Evaluate();

        AttributeSymbol attr = targetObj.GetAttribute(_name, true, _line);
        
        Obj newValue = _newValue.Evaluate();

        if (!attr.Type.AcceptsType(newValue.Object))
        {
            throw new BowTypeError(
                $"'{attr.Object.DisplayName()}' does not accept type '{newValue.Object!.DisplayName()}' in public attribute assignment on line {_line}");
        }
        
        attr.SetValue(newValue); 
    }
}
