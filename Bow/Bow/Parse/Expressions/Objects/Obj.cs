using Parse.Environment;

namespace Parse.Expressions.Objects;

public class Obj
{
    public ObjectSymbol? Object { get; protected init; }
    
    public virtual Obj ExecuteMethod(string name, List<Expression> parameters, bool fromPublic, int line)
    {
        throw new NotImplementedException();
    }
    
    public virtual AttributeSymbol GetAttribute(string name, bool fromPublic, int line)
    {
        throw new NotImplementedException();
    }
    
    public virtual MethodSymbol GetMethod(string name, int line)
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

    public bool AcceptsType(Obj type)
    {
        return Object is not null && type.IsAcceptedBy(Object);
    }
    
    public bool IsAcceptedBy(ObjectSymbol symbol)
    {
        return Object is not null && symbol.AcceptsType(Object);
    }
}
