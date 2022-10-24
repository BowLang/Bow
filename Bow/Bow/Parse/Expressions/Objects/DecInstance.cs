using System.Globalization;
using Errors;
using Parse.Environment;
using Parse.Expressions.Objects.UserObjects;

namespace Parse.Expressions.Objects;

public class DecInstance : Obj
{
    public readonly double Value;
    private const double Tolerance = 0.00001;
    private readonly Dictionary<string, Dictionary<string, dynamic>> _attributes;
    
    public DecInstance(string value, int line)
    {
        Value = double.Parse(value);
        Object = Env.GetObject("dec", line);
        
        _attributes = new Dictionary<string, Dictionary<string, dynamic>>
        {
            { 
                "#value", new Dictionary<string, dynamic>
                {
                    { "value", Value },
                    { "type", "dec" }
                }
            }
        };
    }
    
    public DecInstance(double value, int line)
    {
        Value = value;
        Object = Env.GetObject("dec", line);
        
        _attributes = new Dictionary<string, Dictionary<string, dynamic>>
        {
            { 
                "#value", new Dictionary<string, dynamic>
                {
                    { "value", Value },
                    { "type", "dec" }
                }
            }
        };
    }

    public override Obj ExecuteMethod(string name, List<Expression> parameters, bool fromPublic, int line)
    {
        if (new[] { "-@", "to_str" }.Contains(name))
        {
            if (parameters.Count != 0)
            {
                throw new BowRuntimeError(
                    $"Incorrect number of parameters provided for dec unary operator on line {line}");
            }

            return name switch
            {
                "-" => new DecInstance(-Value, line),
                "to_str" => new StrInstance(Value.ToString(CultureInfo.InvariantCulture), line),
                _ => throw new BowRuntimeError($"Invalid unary operator {name} on line {line}")
            };
        }
        
        if (new[] { "+", "-", "/", "*", ">", ">=", "<", "<=", "=", "!=" }.Contains(name))
        {
            if (parameters.Count != 1)
            {
                throw new BowRuntimeError(
                    $"Incorrect number of parameters provided for dec binary operator on line {line}");
            }
            
            Obj other = parameters[0].Evaluate();

            if (other.GetType() != typeof(DecInstance))
            {
                throw new BowTypeError($"Can't perform dec operation on two different types on line {line}");
            }
            
            DecInstance dec = (DecInstance)other;

            if (new[] { "+", "-", "/", "*" }.Contains(name))
            {
                double arithResult = name switch
                {
                    "+" => Value + dec.Value,
                    "-" => Value - dec.Value,
                    "/" => Value / dec.Value,
                    "*" => Value * dec.Value,
                    _ => throw new BowRuntimeError($"Invalid dec arithmetic operator {name} on line {line}")
                };
                
                return new DecInstance(arithResult, line);
            }

            bool compResult = name switch
            {
                ">"  => Value > dec.Value,
                ">=" => Value >= dec.Value,
                "<"  => Value < dec.Value,
                "<=" => Value <= dec.Value,
                "="  => Math.Abs(Value - dec.Value) < Tolerance,
                "!=" => Math.Abs(Value - dec.Value) < Tolerance,
                _ => throw new BowRuntimeError($"Invalid dec comparison operator {name} on line {line}")
            };
            
            return new BooInstance(compResult, line);
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
            AttributeSymbol attribute = new AttributeSymbol(name, Env.GetObject("dec", line), line, true, true);
            
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
        
        return new UserObjInstance(Object, new List<Expression>(), line).GetAttribute(name, fromPublic, line);
    }
    
    public override string DisplayName()
    {
        return "dec";
    }
    
    public override string DisplayValue()
    {
        return $"{Value}";
    }
}
