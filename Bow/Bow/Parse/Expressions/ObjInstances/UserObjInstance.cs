using Errors;
using Parse.Environment;
using Parse.Statements;

namespace Parse.Expressions.ObjInstances;

public class UserObjInstance : ObjInstance
{
    private readonly Dictionary<string, AttributeSymbol> _attributes;
    private readonly Dictionary<string, MethodSymbol> _methods;

    public UserObjInstance(ObjectSymbol passedObject, List<Expression> parameters, int line)
    {
        Object = passedObject;
        _methods = passedObject.Methods;
        _attributes = CopyAttributes(passedObject.Attributes);
        
        ExecuteMethod("new", parameters, line);
    }
    
    private Dictionary<string, AttributeSymbol> CopyAttributes(Dictionary<string, AttributeSymbol> attributes)
    {
        Dictionary<string, AttributeSymbol> newAttributes = new Dictionary<string, AttributeSymbol>();
        
        foreach (var attribute in attributes)
        {
            newAttributes.Add(attribute.Key, attribute.Value.Copy());
        }
        
        return newAttributes;
    }

    public override ObjInstance ExecuteMethod(string name, List<Expression> parameters, int line)
    {
        MethodSymbol method = GetMethod(name, true, line);

        Dictionary<string, VariableSymbol> passedParams = GetParameters(method, parameters, line);
        
        Env.PushScope(new Env(passedParams));
        ObjInstance? startObj = Env.CurrentInstanceObj;
        Env.CurrentInstanceObj = this;

        ObjInstance returned = new NullInstance();

        try
        {
            foreach (Statement statement in method.Statements)
            {
                statement.Interpret();
            }
        }
        catch (BowReturn ex)
        {
            returned = ex.Object;
        }

        Env.PopScope();
        Env.CurrentInstanceObj = startObj;

        if (name == "new")
        {
            returned = this;
        }
        
        return returned;
    }
    
    public override AttributeSymbol GetAttribute(string name, int line)
    {
        if (!_attributes.ContainsKey(name))
        {
            throw new BowNameError($"{Object!.Name} has no attribute '{name}' on line {line}");
        }

        AttributeSymbol attr = _attributes[name];

        if (Env.CurrentInstanceObj != this && attr.IsPrivate)
        {
            throw new BowNameError($"{Object!.Name} has no public attribute '{name}' on line {line}");
        }

        return attr;
    }

    private MethodSymbol GetMethod(string name, bool fromPublic, int line)
    {
        if (!_methods.ContainsKey(name))
        {
            throw new BowNameError($"{Object!.Name} has no method '{name}' on line {line}");
        }

        MethodSymbol symbol = _methods[name];

        if (fromPublic && symbol.IsPrivate)
        {
            throw new BowNameError($"{Object!.Name} has no public method '{name}' on line {line}");
        }

        return symbol;
    }

    private Dictionary<string, VariableSymbol> GetParameters(MethodSymbol method, List<Expression> givenParameters, int line)
    {
        CheckParameterLength(method, givenParameters, line);

        Dictionary<string, VariableSymbol> parameters = new();
        
        for (int i = 0; i < givenParameters.Count; i++)
        {
            ObjInstance obj = givenParameters[i].Evaluate();

            bool valid = false;
            Tuple<string, List<string>> param = method.Parameters[i];
            List<ObjectSymbol> acceptedTypes = StringsToObjectSymbols(param.Item2, line);
            
            if (acceptedTypes.Any(type => obj.IsAcceptedBy(type)))
            {
                valid = true;
                parameters.Add(param.Item1, new VariableSymbol(param.Item1, obj, line, false));
            }

            if (!valid)
            {
                string validTypes = acceptedTypes[0].DisplayName();

                if (acceptedTypes.Count > 1)
                {
                    validTypes = acceptedTypes.GetRange(1, acceptedTypes.Count - 1).Aggregate(validTypes,
                        (current, type) => current + $", {type.DisplayName()}");
                }

                throw new BowTypeError(
                    $"Incorrect type for parameter {param.Item1} for '{Object!.DisplayName()}::{method.Name}'. Expected '{validTypes}', found '{obj.DisplayName()}' on line {line}");
            }
        }

        return parameters;
    }

    private List<ObjectSymbol> StringsToObjectSymbols(List<string> rTypes, int line)
    {
        List<ObjectSymbol> returnTypes = new();

        foreach (string type in rTypes)
        {
            returnTypes.Add(Env.GetObject(type, line));
        }

        return returnTypes;
    }

    private void CheckParameterLength(MethodSymbol method, List<Expression> givenParams, int line)
    {
        int expected = method.Parameters.Count;
        int given = givenParams.Count;
        string diff = given > expected ? "many" : "few";
        
        if (expected != given)
        {
            throw new BowTypeError(
                $"Too {diff} parameters provided for {Object!.Name}::{method.Name}. Given {given}, expected {expected} on line {line}");
        }
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
