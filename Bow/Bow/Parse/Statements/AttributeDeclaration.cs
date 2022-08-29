using Errors;
using Parse.Environment;
using Parse.Expressions.ObjInstances;

namespace Parse.Statements;

public class AttributeDeclaration : Statement
{
    private readonly bool _isPrivate;
    private readonly string _name;
    private readonly string _type;
    private readonly int _line;
    private readonly bool _isConstant;
    private readonly bool _isStatic;
    
    public AttributeDeclaration(bool isPrivate, string name, string type, bool isConstant, bool isStatic, int line)
    {
        _isPrivate = isPrivate;
        _name  = name;
        _type = type;
        _line  = line;
        _isConstant = isConstant;
        _isStatic = isStatic;
    }

    public override void Interpret()
    {
        ObjectSymbol? obj = Env.CurrentObj;
        
        if (obj is null)
        {
            throw new BowSyntaxError($"Cannot declare attribute {_name} outside of an object on line {_line}");
        }

        AttributeSymbol symbol =
            new AttributeSymbol(_name, Env.GetObject(_type, _line), _line, _isConstant, _isPrivate);

        /*if (_isStatic)
        {
            obj.AddStaticAttribute(symbol);
        }
        else
        {*/
            obj.AddAttribute(symbol);
        //}
    }
}
