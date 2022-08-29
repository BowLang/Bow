using Errors;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.ObjInstances;

namespace Parse.Statements;

public class Assignment : Statement
{
    private readonly string _name;
    private readonly bool _isAttribute;
    private readonly Expression _valueExpression;
    private readonly int _line;
    
    public Assignment(string name, Expression valueExpression, bool isAttribute, int line)
    {
        _name  = name;
        _isAttribute = isAttribute;
        _valueExpression = valueExpression;
        _line  = line;
    }

    public override void Interpret()
    {
        ObjInstance newValue = _valueExpression.Evaluate();

        if (_isAttribute)
        {
            AssignAttribute(newValue);
        }
        else
        {
            AssignVariable(newValue);
        }
    }

    private void AssignAttribute(ObjInstance newValue)
    {
        ObjInstance? obj = Env.CurrentInstanceObj;

        if (obj is null)
        {
            throw new BowSyntaxError($"Cannot access attribute {_name} outside of an object on line {_line}");
        }
        
        AttributeSymbol attribute = obj.GetAttribute(_name, _line);
        
        ObjInstance oldValue = attribute.Object;

        if (attribute.IsConstant && oldValue.GetType() != typeof(NullInstance))
        {
            throw new BowSyntaxError($"Cannot re-assign to constant attribute {_name} on line {_line}");
        }

        if (!attribute.Type.AcceptsType(newValue.Object))
        {
            throw new BowTypeError(
                $"Can't assign {newValue.DisplayName()} to attribute of type {attribute.Type.DisplayName()} on line {_line}");
        }
        
        attribute.SetValue(newValue);
    }

    private void AssignVariable(ObjInstance newValue)
    {
        if (!Env.IsVariableDefined(_name))
        {
            throw new BowSyntaxError($"Unknown variable '{_name}' on line {_line}");
        }
        
        VariableSymbol symbol = Env.GetVariable(_name);
        
        if (symbol.IsConstant)
        {
            throw new BowSyntaxError($"Cannot assign to constant '{_name}' on line {_line}");
        }
        
        ObjInstance oldValue = symbol.Object;

        if (!oldValue.AcceptsType(newValue))
        {
            throw new BowTypeError(
                $"Can't assign {newValue.DisplayName()} to variable of type {oldValue.DisplayName()} on line {_line}");
        }

        symbol.SetValue(newValue);
    }

    public override string Interpret(bool lastInShell)
    {
        Interpret();
        
        return lastInShell ? Env.GetVariable(_name).Object.DisplayValue() : "";
    }
}
