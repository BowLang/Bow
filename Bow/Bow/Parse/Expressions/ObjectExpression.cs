using Errors;
using Parse.Environment;
using Parse.Expressions.ObjInstances;

namespace Parse.Expressions;

public class ObjectExpression : Expression
{
    private readonly string _name;
    private readonly List<Expression> _parameters;
    private readonly int _line;
    
    public ObjectExpression(string name, List<Expression> parameters, int line)
    {
        _name = name;
        _parameters = parameters;
        _line = line;
    }

    public override ObjInstance Evaluate()
    {
        if (new[] { "str", "dec", "boo" }.Contains(_name))
        {
            if (_parameters.Count != 1)
            {
                throw new BowSyntaxError($"Incorrect number of parameters for new {_name} on line {_line}");
            }
        }

        ObjInstance param;

        switch (_name)
        {
            case "str":
                param = _parameters[0].Evaluate();

                if (param.GetType() != typeof(StrInstance))
                {
                    throw new BowTypeError($"Argument for new Str must be of type Str on line {_line}");
                }

                return param;
            case "dec":
                param = _parameters[0].Evaluate();

                if (param.GetType() != typeof(DecInstance))
                {
                    throw new BowTypeError($"Argument for new Dec must be of type Dec on line {_line}");
                }

                return param;
            case "boo":
                param = _parameters[0].Evaluate();

                if (param.GetType() != typeof(BooInstance))
                {
                    throw new BowTypeError($"Argument for new Boo must be of type Boo on line {_line}");
                }

                return param;
            default:
                ObjectSymbol symbol = Env.GetObject(_name, _line);
                return new UserObjInstance(symbol, _parameters, _line);
        }
    }
}