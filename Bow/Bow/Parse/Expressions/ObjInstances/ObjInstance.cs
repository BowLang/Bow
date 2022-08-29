using Parse.Environment;

namespace Parse.Expressions.ObjInstances;

public class ObjInstance
{
    public ObjectSymbol? Object { get; init; }
    
    public virtual ObjInstance ExecuteMethod(string name, List<Expression> parameters, int line)
    {
        throw new NotImplementedException();
    }
    
    public virtual AttributeSymbol GetAttribute(string name, int line)
    {
        throw new NotImplementedException();
    }

    public virtual string DisplayName()
    {
        throw new NotImplementedException();
    }

    public virtual string DisplayValue()
    {
        throw new NotImplementedException();
    }

    public bool AcceptsType(ObjInstance type)
    {
        return Object is not null && type.IsAcceptedBy(Object);
    }
    
    public bool IsAcceptedBy(ObjectSymbol symbol)
    {
        return Object is not null && symbol.AcceptsType(Object);
    }
}
