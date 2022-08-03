using Errors;
using Parse.Expressions;

namespace Parse.Environment;

public class Env
{
    private static readonly List<Env> Scopes = new() { new Env() };
    private readonly Dictionary<string, VariableSymbol> _variables;
    private readonly Dictionary<string, FunctionSymbol> _functions;

    public Env()
    {
        _variables = new Dictionary<string, VariableSymbol>();
        _functions = new Dictionary<string, FunctionSymbol>();
    }

    public Env(Dictionary<string, VariableSymbol> variables)
    {
        _variables = variables;
        _functions = new Dictionary<string, FunctionSymbol>();
    }

    public static void PushScope(Env env)
    {
        Scopes.Insert(0, env);
    }
    
    public static void PopScope()
    {
        Scopes.RemoveAt(0);
    }

    public static void AddVariable(VariableSymbol symbol)
    {
        if (FunctionExpression.IsBuiltinFunction(symbol.Name))
        {
            throw new BowNameError($"Unable to redefine built-in function '{symbol.Name}'");
        }
        
        Env scope = Scopes[0];
        
        if (!scope._variables.ContainsKey(symbol.Name))
        {
            scope._variables.Add(symbol.Name, symbol);
        }
    }

    public static VariableSymbol GetVariable(string name)
    {
        foreach (Env scope in Scopes)
        {
            if (scope._variables.ContainsKey(name))
            {
                return scope._variables[name];
            }
        }
        
        throw new BowNameError($"Variable '{name}' not found");
    }
    
    public static bool IsVariableDefined(string name)
    {
        try
        {
            GetVariable(name);
            return true;
        }
        catch (BowNameError)
        {
            return false;
        }
    }

    private static void OutputVariables()
    {
        Console.WriteLine("\nVariables:");
        foreach (Env scope in Scopes)
        {
            foreach (KeyValuePair<string, VariableSymbol> pair in scope._variables)
            {
                string value = pair.Value.Literal.DisplayValue;
                
                Console.WriteLine($"{pair.Key} = {value}");
            }
        }
    }

    public static void AddFunction(FunctionSymbol symbol)
    {
        if (FunctionExpression.IsBuiltinFunction(symbol.Name))
        {
            throw new BowNameError($"Unable to redefine built-in function '{symbol.Name}'");
        }
        
        Env scope = Scopes[0];
        
        if (!scope._functions.ContainsKey(symbol.Name))
        {
            scope._functions.Add(symbol.Name, symbol);
        }
    }

    public static FunctionSymbol GetFunction(string name, int line)
    {
        foreach (Env scope in Scopes)
        {
            if (scope._functions.ContainsKey(name))
            {
                return scope._functions[name];
            }
        }
        
        throw new BowNameError($"Function '{name}' not found on line {line}");
    }
    
    public static bool IsFunctionDefined(string name, int line)
    {
        try
        {
            GetFunction(name, line);
            return true;
        }
        catch (BowNameError)
        {
            return FunctionExpression.IsBuiltinFunction(name);
        }
    }

    private static void OutputFunctions()
    {
        Console.WriteLine("\nFunctions:");
        foreach (Env scope in Scopes)
        {
            foreach (KeyValuePair<string, FunctionSymbol> pair in scope._functions)
            {
                Console.WriteLine(pair.Key);
            }
        }
    }

    public static void Output()
    {
        OutputVariables();
        OutputFunctions();
    }
}
