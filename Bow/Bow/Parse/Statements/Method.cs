using Errors;
using Parse.Environment;

namespace Parse.Statements;

public class Method : Statement
{
    private readonly bool _isPrivate;
    private readonly string _name;
    private readonly List<Tuple<string, List<string>>> _parameters;
    private readonly List<string> _returnTypes;
    private readonly List<Statement> _statements;
    private readonly bool _isStatic;
    private readonly int _line;
    
    public Method(bool isPrivate, string name, List<Tuple<string, List<string>>> parameters, List<string> returnTypes,
        List<Statement> statements, bool isStatic, int line)
    {
        _isPrivate = isPrivate;
        _name = name;
        _parameters = parameters;
        _returnTypes = returnTypes;
        _statements = statements;
        _isStatic = isStatic;
        _line = line;
    }

    public override void Interpret()
    {
        ObjectSymbol? obj = Env.CurrentObj;

        if (obj is null)
        {
            throw new BowSyntaxError($"Cannot declare method {_name} outside of an object on line {_line}");
        }

        MethodSymbol symbol = new MethodSymbol(_isPrivate, _name, _parameters, _returnTypes,
            _statements, _line);

        if (_isStatic)
        {
            obj.AddStaticMethod(symbol);
        }
        else
        {
            obj.AddMethod(symbol);
        }
    }
}
