using Errors;
using Parse.Expressions;
using Parse.Expressions.Objects;
using Parse.Expressions.Objects.UserObjects;

namespace Parse.Environment;

public class Env
{
    public static int NestLevel;
    public static ObjectSymbol? CurrentObj { get; set; }
    public static Obj? CurrentInstanceObj { get; set; }
    private static readonly List<Env> Scopes = new() { new Env() };
    private readonly Dictionary<string, ObjectSymbol>   _objects;
    private readonly Dictionary<string, VariableSymbol> _variables;
    private readonly Dictionary<string, FunctionSymbol> _functions;

    public Env()
    {
        _objects = new Dictionary<string, ObjectSymbol>();
        _variables = new Dictionary<string, VariableSymbol>();
        _functions = new Dictionary<string, FunctionSymbol>();
    }

    public Env(Dictionary<string, VariableSymbol> variables)
    {
        _variables = variables;
        _objects   = new Dictionary<string, ObjectSymbol>();
        _functions = new Dictionary<string, FunctionSymbol>();
    }

    public static void PushScope(Env env)
    {
        NestLevel++;
        Scopes.Insert(0, env);
    }
    
    public static void PopScope()
    {
        NestLevel--;
        Scopes.RemoveAt(0);
    }

    public static void SetNestLevel(int level)
    {
        while (NestLevel > level)
        {
            PopScope();
        }
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

    public static VariableSymbol GetVariable(string name, int line)
    {
        foreach (Env scope in Scopes)
        {
            if (scope._variables.ContainsKey(name))
            {
                return scope._variables[name];
            }
        }
        
        throw new BowNameError($"Variable '{name}' not found on line {line}");
    }
    
    public static bool IsVariableDefined(string name)
    {
        try
        {
            GetVariable(name, 0);
            return true;
        }
        catch (BowNameError)
        {
            return false;
        }
    }

    public static bool IsVariableDefinedLocally(string name)
    {
        return Scopes[0]._variables.ContainsKey(name);
    }

    private static void OutputVariables()
    {
        Console.WriteLine("\nVariables:");
        foreach (Env scope in Scopes)
        {
            foreach (KeyValuePair<string, VariableSymbol> pair in scope._variables)
            {
                string value = pair.Value.Object.DisplayValue();
                
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

    public static bool IsMethodDefined(string name)
    {
        return CurrentInstanceObj?.Object != null &&
               (CurrentInstanceObj.Object.IsStaticMethodDefined(name) ||
                CurrentInstanceObj.Object.Methods.ContainsKey(name));
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
    
    public static void AddObject(ObjectSymbol symbol)
    {
        Env scope = Scopes[0];
        
        if (!scope._objects.ContainsKey(symbol.Name))
        {
            scope._objects.Add(symbol.Name, symbol);
        }
    }
    
    public static ObjectSymbol GetObject(string name, int line)
    {
        foreach (Env scope in Scopes)
        {
            if (scope._objects.ContainsKey(name))
            {
                return scope._objects[name];
            }
        }
        
        throw new BowNameError($"Object '{name}' not found on line {line}");
    }
    
    public static bool IsObjectDefined(string name, int line)
    {
        try
        {
            GetObject(name, line);
            return true;
        }
        catch (BowNameError)
        {
            return false;
        }
    }

    private static void OutputObjects()
    {
        Console.WriteLine("\nObjects:");
        foreach (Env scope in Scopes)
        {
            foreach (KeyValuePair<string, ObjectSymbol> pair in scope._objects)
            {
                string name = pair.Key;
                ObjectSymbol obj = pair.Value; 
                
                Dictionary<string, AttributeSymbol> staticAttributes = obj.Static.GetAttributes();
                Dictionary<string, MethodSymbol> staticMethods = obj.Static.GetMethods();

                Console.WriteLine($"  {name}:");
                
                if (obj.Attributes.Count != 0)
                {
                    Console.WriteLine("    Attributes:");
                    foreach (KeyValuePair<string, AttributeSymbol> attribute in obj.Attributes)
                    {
                        string visibility = attribute.Value.IsPrivate ? "private" : "public";
                        Console.WriteLine($"      {visibility} {attribute.Key}");
                    }
                }
                
                if (staticAttributes.Keys.Count != 0)
                {
                    Console.WriteLine("\n    Static Attributes:");
                    foreach (KeyValuePair<string, AttributeSymbol> attribute in staticAttributes)
                    {
                        string visibility = attribute.Value.IsPrivate ? "Private" : "Public";
                        Console.WriteLine(
                            $"      {visibility} {attribute.Key} = {attribute.Value.Object.DisplayValue()}");
                    }
                }

                if (obj.Methods.Count != 0)
                {
                    Console.WriteLine("\n    Methods:");
                    foreach (KeyValuePair<string, MethodSymbol> method in obj.Methods)
                    {
                        string visibility = method.Value.IsPrivate ? "Private" : "Public";
                        Console.WriteLine($"      {visibility} {method.Key}");
                    }
                }
                
                if (staticMethods.Keys.Count != 0)
                {
                    Console.WriteLine("\n    Static Methods:");
                    foreach (KeyValuePair<string, MethodSymbol> method in staticMethods)
                    {
                        string visibility = method.Value.IsPrivate ? "Private" : "Public";
                        Console.WriteLine($"      {visibility} {method.Key}");
                    }
                }
            }
        }
    }

    public static void Output()
    {
        Console.WriteLine("\n");
        OutputVariables();
        OutputFunctions();
        OutputObjects();
    }
}
