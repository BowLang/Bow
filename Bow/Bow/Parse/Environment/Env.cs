using Errors;

namespace Parse.Environment;

public class Env
{
    private static readonly List<Env> Scopes = new() { new Env() };
    private readonly Dictionary<string, Symbol> _variables; 

    public Env()
    {
        _variables = new Dictionary<string, Symbol>();
    }

    public static void PushScope(Env env)
    {
        Scopes.Insert(0, env);
    }
    
    public static void PopScope()
    {
        Scopes.RemoveAt(0);
    }

    public static void AddVariable(Symbol symbol)
    {
        Env scope = Scopes[0];
        
        if (!scope._variables.ContainsKey(symbol.Name))
        {
            scope._variables.Add(symbol.Name, symbol);
        }
    }

    public static Symbol GetVariable(string name)
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

    public static void OutputVariables()
    {
        Console.WriteLine("\nVariables:");
        foreach (Env scope in Scopes)
        {
            foreach (KeyValuePair<string, Symbol> pair in scope._variables)
            {
                string value = pair.Value.Literal.DisplayValue;
                
                Console.WriteLine($"{pair.Key} = {value}");
            }
        }
    }
}
