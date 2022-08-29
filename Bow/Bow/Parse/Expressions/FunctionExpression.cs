using System.Reflection;
using Errors;
using Parse.Statements;
using Parse.Environment;
using Parse.Expressions.ObjInstances;

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
    
    public override ObjInstance Evaluate()
    {
        bool isBuiltin = IsBuiltinFunction(_name);
        
        if (isBuiltin || Env.IsFunctionDefined(_name, _line))
        {
            return ExecuteFunction(isBuiltin);
        }
        
        return ExecuteMethod();
    }

    private ObjInstance ExecuteMethod()
    {
        if (Env.CurrentInstanceObj is null)
        {
            throw new BowNameError($"Could not find function '{_name}' on line {_line}");
        }
        
        return Env.CurrentInstanceObj.ExecuteMethod(_name, _parameters, _line);
    }
    
    private ObjInstance ExecuteFunction(bool isBuiltin)
    {
        if (isBuiltin)
        {
            return ExecuteBuiltinFunction();
        }
        
        FunctionSymbol function = Env.GetFunction(_name, _line);
        
        return ExecuteUserFunction(function.Parameters, function.ReturnTypes, function.Statements);
    }

    private ObjInstance ExecuteUserFunction(List<Tuple<string, List<string>>> expectedParameters, List<string> returnTypes,
        List<Statement> statements, bool isMethod = false)
    {
        string type = isMethod ? "method" : "function";
        
        CheckParametersLength(expectedParameters);
        
        Dictionary<string, VariableSymbol> parameters = new();
        
        foreach (var param in _parameters.Select((value, i) => new { i, value }))
        {
            ObjInstance paramObj = param.value.Evaluate();

            string name = expectedParameters[param.i].Item1;
            
            if (!InTypeList(expectedParameters[param.i].Item2, paramObj))
            {
                throw new BowSyntaxError($"Incorrect type for parameter '{name}' of {type} '{_name}' on line {_line}");
            }
            
            parameters.Add(name, new VariableSymbol(name, paramObj, _line, false));
        }

        ObjInstance value = new NullInstance();
        
        int nestLevel = Env.NestLevel;
        
        Env.PushScope(new Env(parameters));

        try
        {
            foreach (Statement statement in statements)
            {
                statement.Interpret();
            }
        }
        catch (BowReturn ex)
        {
            value = ex.Object;
        }
        finally
        {
            Env.SetNestLevel(nestLevel);
        }

        CheckReturnTypes(value, returnTypes, type);

        return value;
    }

    private void CheckParametersLength(List<Tuple<string, List<string>>> accepted)
    {
        if (_parameters.Count < accepted.Count)
        {
            throw new BowSyntaxError($"Not enough parameters for {_name} on line {_line}");
        }
        
        if (_parameters.Count > accepted.Count)
        {
            throw new BowSyntaxError($"Too many parameters for {_name} on line {_line}");
        }
    }
    
    private void CheckReturnTypes(ObjInstance value, List<string> types, string type)
    {
        string titledType = type[0].ToString().ToUpper() + type[1..^1];
        
        if (new NullInstance().AcceptsType(value) && types.Count != 0)
        {
            throw new BowTypeError(
                $"{titledType} call unexpectedly did not return anything on line {_line}");
        }
        
        if (new NullInstance().AcceptsType(value))
        {
            return;
        }

        if (!InTypeList(types, value))
        {
            throw new BowTypeError($"{titledType} call did not return a correct type on line {_line}");
        }
    }

    private bool InTypeList(List<string> types, ObjInstance obj)
    {
        foreach (ObjectSymbol type in StringsToObjectSymbols(types))
        {
            if (obj.IsAcceptedBy(type))
            {
                return true;
            }
        }

        return false;
    }

    private List<ObjectSymbol> StringsToObjectSymbols(List<string> rTypes)
    {
        List<ObjectSymbol> returnTypes = new();

        foreach (string type in rTypes)
        {
            returnTypes.Add(Env.GetObject(type, _line));
        }

        return returnTypes;
    }

    private ObjInstance ExecuteBuiltinFunction()
    {
        const BindingFlags flags = BindingFlags.InvokeMethod |
                                   BindingFlags.Public |
                                   BindingFlags.Static;
        
        MethodInfo? func =  typeof(Builtins).GetMethod(_name, flags);

        if (func == null)
        {
            throw new BowNameError($"Unknown function '{_name}' on line {_line}");
        }
        
        object[] parameters = _parameters.Select(param => param.Evaluate()).ToArray<object>();

        try
        {
            Builtins.Line = _line;
            return (ObjInstance)(func.Invoke(null, parameters) ?? new NullInstance());
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
    public static int Line { get; set; }
    
    public static ObjInstance output(ObjInstance value)
    {
        value = value.ExecuteMethod("to_str", new List<Expression>(), Line);

        if (value.GetType() != typeof(StrInstance))
        {
            throw new BowTypeError($"Object's ::to_str implementation did not return a str object on line {Line}");
        }

        string str = ((StrInstance)value).Value;
        
        Console.WriteLine(str);
        return new NullInstance();
    }
    
    public static ObjInstance input(ObjInstance? message=null)
    {
        string str = "";
        
        if (message is not null)
        {
            message = message.ExecuteMethod("to_str", new List<Expression>(), Line);

            if (message.GetType() != typeof(StrInstance))
            {
                throw new BowTypeError($"Object's ::to_str implementation did not return a str object on line {Line}");
            }

            str = ((StrInstance)message).Value;
        }
        
        Console.Write(str);
        return new StrInstance(Console.ReadLine() ?? "", Line);
    }
}
