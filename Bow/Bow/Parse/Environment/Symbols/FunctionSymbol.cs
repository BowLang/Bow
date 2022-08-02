using Parse.Statements;

namespace Parse.Environment;

public class FunctionSymbol
{
    public string Name { get; }
    public List<Tuple<string, List<string>>> Parameters { get; }
    public List<string> ReturnTypes { get; }
    public List<Statement> Statements { get; }
    public int Line { get; }

    public FunctionSymbol(string name, List<Tuple<string, List<string>>> parameters, List<string> returnTypes,
        List<Statement> statements, int line)
    {
        Name = name;
        Parameters = parameters;
        ReturnTypes = returnTypes;
        Statements = statements;
        Line = line;
    }
}
