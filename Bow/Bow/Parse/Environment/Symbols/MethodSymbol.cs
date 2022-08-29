using Parse.Statements;

namespace Parse.Environment;

public class MethodSymbol
{
    public bool IsPrivate { get; }
    public string Name { get; }
    public List<Tuple<string, List<string>>> Parameters { get; }
    public List<string> ReturnTypes { get; }
    public List<Statement> Statements { get; }
    public int Line { get; }

    public MethodSymbol(bool isPrivate, string name, List<Tuple<string, List<string>>> parameters,
        List<string> returnTypes, List<Statement> statements, int line)
    {
        Parameters = parameters;
        ReturnTypes = returnTypes;
        Statements = statements;
        Name = name;
        Line = line;
        IsPrivate = isPrivate;
    }
}
