using System.Reflection;
using Errors;
using Tokenise;
using Parse.Statements;
using Parse.Environment;
using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class FunctionExpression : Expression
{
    private readonly string _name;
    private readonly List<Expression> _parameters;
    private readonly int _line;
    
    public FunctionExpression(string name, List<Expression> parameters, int line)
    {
        _name = name;
        _parameters = parameters;
        _line = line;
    }
    
    public override Literal Evaluate()
    {
        bool isBuiltin = IsBuiltinFunction(_name);

        return ExecuteFunction(isBuiltin);
    }
    
    private void CheckParametersLength(List<Tuple<string, List<string>>> accepted)
    {
        if (_parameters.Count < accepted.Count)
        {
            throw new BowSyntaxError($"Not enough parameters for function {_name} on line {_line}");
        }
        
        if (_parameters.Count > accepted.Count)
        {
            throw new BowSyntaxError($"Too many parameters for function {_name} on line {_line}");
        }
    }
    
    private Literal ExecuteFunction(bool isBuiltin)
    {
        return isBuiltin ? ExecuteBuiltinFunction() : ExecuteUserFunction();
    }

    private Literal ExecuteUserFunction()
    {
        FunctionSymbol function = Env.GetFunction(_name, _line);
        
        CheckParametersLength(function.Parameters);
        
        Dictionary<string, VariableSymbol> parameters = new();
        
        foreach (var param in _parameters.Select((value, i) => new { i, value }))
        {
            Literal paramLiteral = param.value.Evaluate();

            string name = function.Parameters[param.i].Item1;
            
            if (!function.Parameters[param.i].Item2.Contains(paramLiteral.Type))
            {
                throw new BowSyntaxError($"Incorrect type for parameter '{name}' of function '{_name}' on line {_line}");
            }
            
            parameters.Add(name, new VariableSymbol(name, paramLiteral, _line, false));
        }

        Literal? value = null;
        
        int nestLevel = Env.NestLevel;
        
        Env.PushScope(new Env(parameters));

        try
        {
            foreach (Statement statement in function.Statements)
            {
                statement.Interpret();
            }
        }
        catch (BowReturn ex)
        {
            value = ex.Literal;
        }
        finally
        {
            Env.SetNestLevel(nestLevel);
        }

        CheckReturnTypes(value, function.ReturnTypes);
        
        if (value is null)
        {
            return new NullReturn();
        }

        return value.Type switch
        {
            TokenType.StrLiteral => new StrLiteral(value.Value),
            TokenType.BooLiteral => new BooLiteral(value.Value),
            TokenType.DecLiteral => new DecLiteral(value.Value),
            _ => throw new BowRuntimeError($"Variable expression contains unknown type {value.Type} on line {_line}")
        };
    }
    
    private void CheckReturnTypes(Literal? value, List<string> types)
    {
        if (value is null && types.Count != 0)
        {
            throw new BowTypeError($"Function call unexpectedly did not return anything on line {_line}");
        }
        
        if (value is null)
        {
            return;
        }

        if (!types.Contains(value.Type))
        {
            throw new BowTypeError($"Function call did not return a correct type on line {_line}");
        }
    }

    private Literal ExecuteBuiltinFunction()
    {
        Type builtins = typeof(Builtins);

        const BindingFlags flags = BindingFlags.InvokeMethod |
                                   BindingFlags.Public |
                                   BindingFlags.Static;
        
        MethodInfo? func = builtins.GetMethod(_name, flags);

        if (func == null)
        {
            throw new BowNameError($"Unknown function '{_name}' on line {_line}");
        }
        
        object[] parameters = _parameters.Select(param => param.Evaluate().Value).ToArray();

        try
        {
            return (Literal)(func.Invoke(null, parameters) ?? new NullReturn());
        }
        catch (TargetParameterCountException)
        {
            throw new BowSyntaxError($"Incorrect number of parameters for function '{_name}' on line {_line}");
        }
        catch (ArgumentException)
        {
            throw new BowTypeError($"Incorrect type for parameter on line {_line}");
        }
    }

    public static bool IsBuiltinFunction(string name)
    {
        const BindingFlags flags = BindingFlags.InvokeMethod |
                                   BindingFlags.Public |
                                   BindingFlags.Static;
        
        MethodInfo? func = typeof(Builtins).GetMethod(name, flags);

        return func != null;
    }
}

public class Builtins
{
    public static Literal output(string value)
    {
        Console.WriteLine(value);
        return new NullReturn();
    }
    
    public static Literal input(string message="")
    {
        Console.Write(message);
        return new StrLiteral(Console.ReadLine() ?? "");
    }
}
