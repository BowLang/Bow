using Errors;
using Parse.Environment;
using Parse.Expressions.Objects.UserObjects;

namespace Parse.Expressions.Objects;

public class StrInstance : Obj
{
    public readonly string Value;
    private readonly Dictionary<string, Dictionary<string, dynamic>> _attributes;

    public StrInstance(string value, int line)
    {
        Value = value;
        Object = Env.GetObject("str", line);
        
        _attributes = new Dictionary<string, Dictionary<string, dynamic>>
        {
            { 
                "#value", new Dictionary<string, dynamic>
                {
                    { "value", Value },
                    { "type", "str" }
                }
            },
            {
                "#len", new Dictionary<string, dynamic>
                {
                    { "value", Value.Length },
                    { "type", "int" }
                }
            }
        };
    }

    public override Obj ExecuteMethod(string name, List<Expression> parameters, bool fromPublic, int line)
    {
        if (new[] { "to_str", "to_dec" }.Contains(name))
        {
            if (parameters.Count != 0)
            {
                throw new BowRuntimeError(
                    $"Incorrect number of parameters provided for str unary operator on line {line}");
            }

            return name switch
            {
                "to_str" => this,
                "to_dec" => new DecInstance(Value, line),
                _ => throw new BowRuntimeError($"Invalid unary operator {name} on line {line}")
            };
        }
        
        if (new[] { "+", "=", "!="}.Contains(name))
        {
            if (parameters.Count != 1)
            {
                throw new BowRuntimeError(
                    $"Incorrect number of parameters provided for str binary operator on line {line}");
            }
            
            Obj other = parameters[0].Evaluate();

            if (other.GetType() != typeof(StrInstance))
            {
                throw new BowTypeError($"Can't perform str operation on two different types on line {line}");
            }
            
            StrInstance str = (StrInstance)other;

            if (name == "+")
            {
                return new StrInstance(Value + str.Value, line);
            }

            bool result = name switch
            {
                "="  => Value == str.Value,
                "!=" => Value != str.Value,
                _ => throw new BowRuntimeError($"Unknown operator {name} on line {line}")
            };

            return new BooInstance(result, line);
        }

        if (Object is null)
        {
            throw new BowRuntimeError($"DecInstance Object is null on line {line}");
        }

        return new UserObjInstance(Object, new List<Expression>(), line).ExecuteMethod(name, parameters, fromPublic,
            line);
    }

    public override AttributeSymbol GetAttribute(string name, bool fromPublic, int line)
    {
        if (_attributes.ContainsKey(name))
        {
            Dictionary<string, dynamic> attr = _attributes[name];
            AttributeSymbol attribute = new AttributeSymbol(name, Env.GetObject("str", line), line, true, true);
            
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

        throw new BowNameError($"Unknown str attribute {name} on line {line}");
    }
    
    public override string DisplayName()
    {
        return "str";
    }
    
    public override string DisplayValue()
    {
        return $"\"{Value}\"";
    }
}
