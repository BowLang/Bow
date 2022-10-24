using Errors;
using Parse.Expressions.Objects;
using Parse.Expressions.Objects.UserObjects;

namespace Parse.Environment;

public class ObjectSymbol
{
    public readonly string Name;
    private readonly int _line;
    public readonly ObjectSymbol? Super;
    public bool Inherited => Super is not null;
    public readonly Dictionary<string, AttributeSymbol> Attributes;
    public readonly Dictionary<string, MethodSymbol> Methods;
    public readonly Dictionary<string, AttributeSymbol> StaticAttributes;
    public readonly Dictionary<string, MethodSymbol> StaticMethods;
    public readonly StaticObject Static;

    public ObjectSymbol(string name, ObjectSymbol? super, int line)
    {
        Name = name;
        _line = line;
        Attributes = new Dictionary<string, AttributeSymbol>();
        Methods = new Dictionary<string, MethodSymbol>();
        StaticAttributes = new Dictionary<string, AttributeSymbol>();
        StaticMethods = new Dictionary<string, MethodSymbol>();
        Super = super;
        Static = new StaticObject(this, StaticAttributes, StaticMethods);
    }

    public bool IsAttributeDefined(string name)
    {
        return Attributes.ContainsKey(name) || StaticAttributes.ContainsKey(name);
    }

    public void AddAttribute(AttributeSymbol symbol)
    {
        if (IsAttributeDefined(symbol.Name))
        {
            throw new BowNameError($"Cannot redefine attribute {symbol.Name} on line{_line}");
        }

        Attributes.Add(symbol.Name, symbol);
    }

    public void AddStaticAttribute(AttributeSymbol symbol, Obj value)
    {
        if (IsAttributeDefined(symbol.Name))
        {
            throw new BowNameError($"Cannot redefine attribute {symbol.Name} on line{_line}");
        }
        
        symbol.SetValue(value);
        StaticAttributes.Add(symbol.Name, symbol);
    }

    public void AddMethod(MethodSymbol symbol)
    {
        Methods.Add(symbol.Name, symbol);
    }

    public void AddStaticMethod(MethodSymbol symbol)
    {
        StaticMethods.Add(symbol.Name, symbol);
    }

    public bool IsStaticAttributeDefined(string name)
    {
        return StaticAttributes.ContainsKey(name);
    }

    public bool IsStaticMethodDefined(string name)
    {
        return StaticMethods.ContainsKey(name);
    }

    public bool AcceptsType(ObjectSymbol? other)
    {
        return other is not null && (Name == other.Name || AcceptsType(other.Super));
    }

    public string DisplayName()
    {
        string super = Super is null ? "" : $"{Super.DisplayName()}:";
        return $"{super}{Name}";
    }
}
