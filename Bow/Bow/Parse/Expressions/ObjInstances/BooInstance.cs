using Errors;
using Parse.Environment;

namespace Parse.Expressions.ObjInstances;

public class BooInstance : ObjInstance
{
    public readonly bool Value;
    private readonly Dictionary<string, Dictionary<string, dynamic>> _attributes;
    
    public BooInstance(string value, int line)
    {
        Value = bool.Parse(value);
        Object = Env.GetObject("boo", line);
        
        _attributes = new Dictionary<string, Dictionary<string, dynamic>>
        {
            { 
                "#value", new Dictionary<string, dynamic>
                {
                    { "value", Value },
                    { "type", "str" }
                }
            }
        };
    }
    
    public BooInstance(bool value, int line)
    {
        Value = value;
        Object = Env.GetObject("boo", line);
        
        _attributes = new Dictionary<string, Dictionary<string, dynamic>>
        {
            { 
                "#value", new Dictionary<string, dynamic>
                {
                    { "value", Value },
                    { "type", "str" }
                }
            }
        };
    }

    public override ObjInstance ExecuteMethod(string name, List<Expression> parameters, int line)
    {
        if (new[] { "!", "to_str" }.Contains(name))
        {
            if (parameters.Count != 0)
            {
                throw new BowRuntimeError(
                    $"Incorrect number of parameters provided for boo unary operator on line {line}");
            }

            return name switch
            {
                "!" => new BooInstance(!Value, line),
                "to_str" => new StrInstance(Value.ToString(), line),
                _ => throw new BowRuntimeError($"Invalid unary operator {name} on line {line}")
            };
        }
        
        if (new[] { "&&", "||", "=", "!="}.Contains(name))
        {
            if (parameters.Count != 1)
            {
                throw new BowRuntimeError(
                    $"Incorrect number of parameters provided for boo binary operator on line {line}");
            }
            
            ObjInstance other = parameters[0].Evaluate();

            if (other.GetType() != typeof(BooInstance))
            {
                throw new BowTypeError($"Can't perform boo operation on two different types on line {line}");
            }
            
            BooInstance boo = (BooInstance)other;

            bool result = name switch
            {
                "&&" => Value && boo.Value,
                "||" => Value || boo.Value,
                "="  => Value == boo.Value,
                "!=" => Value != boo.Value,
                _ => throw new BowRuntimeError($"Unknown operator {name} on line {line}")
            };

            return new BooInstance(result, line);
        }

        if (Object is null)
        {
            throw new BowRuntimeError($"DecInstance Object is null on line {line}");
        }
        
        return new UserObjInstance(Object, new List<Expression>(), line).ExecuteMethod(name, parameters, line);
    }

    public override AttributeSymbol GetAttribute(string name, int line)
    {
        if (_attributes.ContainsKey(name))
        {
            Dictionary<string, dynamic> attr = _attributes[name];
            AttributeSymbol attribute = new AttributeSymbol(name, Env.GetObject("boo", line), line, true, true);
            
            switch (attr["type"])
            {
                case "str":
                    attribute.SetValue(new StrInstance(attr["value"], line));
                    break;
                case "dec":
                    attribute.SetValue(new DecInstance(attr["value"], line));
                    break;
                case "boo":
                    attribute.SetValue(new BooInstance(attr["value"], line));
                    break;
                default:
                    throw new BowRuntimeError($"Unknown type {attr["type"]} on line {line}");
            }

            return attribute;
        }

        if (Object is null)
        {
            throw new BowRuntimeError($"DecInstance Object is null on line {line}");
        }
        
        return new UserObjInstance(Object, new List<Expression>(), line).GetAttribute(name, line);
    }
    
    public override string DisplayName()
    {
        return "boo";
    }
    
    public override string DisplayValue()
    {
        return $"{Value}";
    }
}
