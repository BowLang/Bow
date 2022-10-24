using Errors;
using Parse.Environment;

namespace Parse.Expressions.Objects.UserObjects;

public class StaticObject : UserObject
{
    private readonly ObjectSymbol _symbol;
    
    public StaticObject(ObjectSymbol symbol, Dictionary<string, AttributeSymbol> attrs,
        Dictionary<string, MethodSymbol> methods) : base(attrs, methods)
    {
        _symbol = symbol;
    }

    public override AttributeSymbol GetAttribute(string name, bool fromPublic, int line)
    {
        if (Attributes.ContainsKey(name))
        {
            AttributeSymbol attr = Attributes[name];
            
            if (fromPublic && attr.IsPrivate)
            {
                throw new BowNameError($"{DisplayName()} has no public attribute '{name}' on line {line}");
            }

            return attr;
        }

        throw new BowNameError($"{DisplayName()} does not have static attribute '{name}' on line {line}");
    }

    public override MethodSymbol GetMethod(string name, int line)
    {
        if (Methods.ContainsKey(name))
        {
            return Methods[name];
        }
        
        throw new BowNameError($"{DisplayName()} does not have a static method {name} on line {line}");
    }
    
    public override Obj ExecuteMethod(string name, List<Expression> parameters, bool fromPublic, int line)
    {
        if (name == "new")
        {
            return new UserObjInstance(_symbol, parameters, line);
        }
        
        if (!Methods.ContainsKey(name))
        {
            throw new BowNameError($"{DisplayName()} does not have static method {name} on line {line}");
        }
        
        return RunMethod(name, parameters, fromPublic, line);
    }

    public override string DisplayName()
    {
        return _symbol.Name;
    }

    public override string DisplayValue()
    {
        return $"<{_symbol.Name}>";
    }
}
