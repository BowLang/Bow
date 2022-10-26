using Errors;
using Microsoft.VisualBasic;
using Parse.Expressions.Objects;
using Parse.Expressions.Objects.UserObjects;

namespace Parse.Environment;

public class ObjectSymbol
{
    public readonly string Name;
    private readonly int _line;
    private readonly ObjectSymbol? _super;
    public readonly Dictionary<string, AttributeSymbol> Attributes;
    public readonly Dictionary<string, MethodSymbol> Methods;
    public readonly StaticObject Static;
    private readonly List<ObjectSymbol> _children;

    public ObjectSymbol(string name, ObjectSymbol? super, int line)
    {
        Name = name;
        _line = line;
        Attributes = new Dictionary<string, AttributeSymbol>();
        Methods = new Dictionary<string, MethodSymbol>();
        _super = super;
        Static = new StaticObject(this, new Dictionary<string, AttributeSymbol>(),
            new Dictionary<string, MethodSymbol>());
        _children = new List<ObjectSymbol>();
    }

    public bool IsAttributeDefined(string name)
    {
        return Attributes.ContainsKey(name);
    }

    public bool IsStaticAttributeDefined(string name)
    {
        return Static.IsAttributeDefined(name);
    }

    public void AddAttribute(AttributeSymbol symbol)
    {
        if (IsAttributeDefined(symbol.Name))
        {
            throw new BowNameError($"Cannot redefine attribute {symbol.Name} on line{_line}");
        }

        Attributes.Add(symbol.Name, symbol);
        
        foreach (ObjectSymbol child in _children.Where(child => !child.IsAttributeDefined(symbol.Name)))
        {
            child.AddAttribute(symbol.Copy());
        }
    }

    public void AddStaticAttribute(AttributeSymbol symbol, Obj value)
    {
        if (IsStaticAttributeDefined(symbol.Name))
        {
            throw new BowNameError($"Cannot redefine attribute '{symbol.Name}' on line{_line}");
        }
        
        symbol.SetValue(value);
        Static.AddAttribute(symbol.Name, symbol);

        foreach (ObjectSymbol child in _children.Where(child => !child.IsStaticAttributeDefined(symbol.Name)))
        {
            AttributeSymbol copy = symbol.Copy();
            child.AddStaticAttribute(copy, copy.Object);
        }
    }

    public void AddMethod(MethodSymbol symbol)
    {
        Methods.Add(symbol.Name, symbol);

        foreach (ObjectSymbol child in _children.Where(child => !child.IsMethodDefined(symbol.Name)))
        {
            child.AddMethod(symbol);
        }
    }

    public void AddStaticMethod(MethodSymbol symbol)
    {
        Static.AddMethod(symbol.Name, symbol);

        foreach (ObjectSymbol child in _children.Where(child => !child.IsStaticMethodDefined(symbol.Name)))
        {
            child.AddStaticMethod(symbol);
        }
    }

    private bool IsMethodDefined(string name)
    {
        return Methods.ContainsKey(name);
    }

    public bool IsStaticMethodDefined(string name)
    {
        return Static.IsMethodDefined(name);
    }
    
    public void AddChild(ObjectSymbol child)
    {
        foreach (KeyValuePair<string, AttributeSymbol> attr in Attributes)
        {
            child.Attributes[attr.Key] = attr.Value.Copy();
        }
        
        foreach (KeyValuePair<string, MethodSymbol> method in Methods)
        {
            child.Methods[method.Key] = method.Value;
        }
        
        foreach (KeyValuePair<string, AttributeSymbol> attr in Static.GetAttributes())
        {
            child.Static.AddAttribute(attr.Key, attr.Value.Copy());
        }
        
        foreach (KeyValuePair<string, MethodSymbol> attr in Static.GetMethods())
        {
            child.Static.AddMethod(attr.Key, attr.Value);
        }
        
        _children.Add(child);
    }

    public bool AcceptsType(ObjectSymbol? other)
    {
        return other is not null && (Name == other.Name || AcceptsType(other._super));
    }

    public string DisplayName()
    {
        string super = _super is null ? "" : $"{_super.DisplayName()}:";
        return $"{super}{Name}";
    }
}
