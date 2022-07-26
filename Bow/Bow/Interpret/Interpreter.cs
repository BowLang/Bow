using Parse.Statements;

namespace Interpret;

public class Interpreter
{
    private readonly List<Statement> _statements;
    
    public Interpreter(List<Statement> statements)
    {
        _statements = statements;
    }
    
    public void Interpret()
    {
        foreach (var statement in _statements)
        {
            statement.Interpret();
        }
    }
}
