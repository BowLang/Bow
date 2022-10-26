using Errors;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.Objects;

namespace Parse.Statements;

public class AttributeDeclaration : Statement
{
    private readonly bool _isPrivate;
    private readonly string _name;
    private readonly string _type;
    private readonly int _line;
    private readonly bool _isConstant;
    private readonly bool _isStatic;
    private readonly Expression? _value;
    
    public AttributeDeclaration(bool isPrivate, string name, string type, bool isConstant, bool isStatic, int line, Expression? value = null)
    {
        _isPrivate = isPrivate;
        _name  = name;
        _type = type;
        _line  = line;
        _isConstant = isConstant;
        _isStatic = isStatic;
        _value = value;
    }

    public override void Interpret()
    {
        ObjectSymbol? obj = Env.CurrentObj;
        
        if (obj is null)
        {
            throw new BowSyntaxError($"Cannot declare attribute {_name} outside of an object on line {_line}");
        }

        if ((!_isStatic && obj.IsAttributeDefined(_name)) || (_isStatic && obj.IsStaticAttributeDefined(_name)))
        {
            string msg = _isStatic ? " static " : "n ";
            throw new BowNameError($"{obj.DisplayName()} already has a{msg} attribute named {_name} on line {_line}");
        }

        AttributeSymbol symbol =
            new AttributeSymbol(_name, Env.GetObject(_type, _line), _line, _isConstant, _isPrivate);

        if (_isStatic)
        {
            if (_value is null)
            {
                throw new BowRuntimeError($"_value is null on line {_line}");
            }

            Obj value = _value.Evaluate();

            if (!symbol.Type.AcceptsType(value.Object))
            {
                throw new BowTypeError(
                    $"Can't assign {value.DisplayName()} to attribute of type {symbol.Type.DisplayName()} on line {_line}");
            }
            
            obj.AddStaticAttribute(symbol, value);
        }
        else
        {
            obj.AddAttribute(symbol);
        }
    }
}
