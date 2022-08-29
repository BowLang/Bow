using Errors;

namespace Parse.Environment;

public class ObjectSymbol
{
    public bool Inherited => Super is not null;
    public readonly string Name;
    private readonly int _line;
    public readonly Dictionary<string, AttributeSymbol> Attributes;
    public readonly Dictionary<string, MethodSymbol> Methods;
    public readonly ObjectSymbol? Super;

    public ObjectSymbol(string name, ObjectSymbol? super, int line)
    {
        Name = name;
        _line = line;
        Attributes = new Dictionary<string, AttributeSymbol>();
        Methods = new Dictionary<string, MethodSymbol>();
        if (super is not null) Super = super;
    }

    private bool IsAttributeDefined(string name)
    {
        return Attributes.ContainsKey(name);
    }

    public void AddAttribute(AttributeSymbol symbol)
    {
        if (IsAttributeDefined(symbol.Name))
        {
            throw new BowNameError($"Cannot redefine attribute {symbol.Name} on line{_line}");
        }

        Attributes.Add(symbol.Name, symbol);
    }

    public void AddMethod(MethodSymbol symbol)
    {
        Methods.Add(symbol.Name, symbol);
    }

    /*public bool IsStaticAttributeDefined(string name)
    {
        return Env.IsStaticAttributeDefined(Name, name);
    }

    public bool IsStaticMethodDefined(string name)
    {
        return Env.IsStaticMethodDefined(Name, name);
    }*/

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
