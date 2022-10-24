using Errors;
using Parse.Environment;
using Parse.Statements;

namespace Parse.Expressions.Objects.UserObjects;

public class UserObjInstance : UserObject
{
    public UserObjInstance(ObjectSymbol passedObject, List<Expression> parameters, int line) : base(
        CopyAttributes(passedObject.Attributes), passedObject.Methods)
    {
        Object = passedObject;
        
        ExecuteMethod("new", parameters, true, line);
    }
    
    private static Dictionary<string, AttributeSymbol> CopyAttributes(Dictionary<string, AttributeSymbol> attributes)
    {
        Dictionary<string, AttributeSymbol> newAttributes = new Dictionary<string, AttributeSymbol>();
        
        foreach (var attribute in attributes)
        {
            newAttributes.Add(attribute.Key, attribute.Value.Copy());
        }
        
        return newAttributes;
    }

    public override Obj ExecuteMethod(string name, List<Expression> parameters, bool fromPublic, int line)
    {
        return RunMethod(name, parameters, fromPublic, line);
    }

    public override string DisplayName()
    {
        return Object!.DisplayName();
    }

    public override string DisplayValue()
    {
        return $"<{Object!.Name}>";
    }
}
