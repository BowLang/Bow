using Errors;
using Tokenise;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.Literals;

namespace Parse.Statements;

public class Function : Statement
{
    private readonly string _name;
    private readonly List<Tuple<string, List<string>>> _parameters;
    private readonly List<string> _returnTypes;
    private readonly List<Statement> _statements;
    private readonly int _line;
    
    public Function(string name, List<Tuple<string, List<string>>> parameters, List<string> returnTypes,
        List<Statement> statements, int line)
    {
        _name = name;
        _parameters = parameters;
        _returnTypes = returnTypes;
        _statements = statements;
        _line = line;
    }

    public override void Interpret()
    {
        FunctionSymbol symbol = new FunctionSymbol(_name, _parameters, _returnTypes, _statements, _line);
        Env.AddFunction(symbol);
    }
}
