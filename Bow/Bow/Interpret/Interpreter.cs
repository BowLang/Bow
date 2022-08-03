using Parse.Statements;

namespace Interpret;

public class Interpreter
{
    private readonly List<Statement> _statements;
    
    public Interpreter(List<Statement> statements)
    {
        _statements = statements;
    }
    
    public void Interpret(bool inShell=false)
    {
        foreach (var statement in _statements.SkipLast(inShell ? 1 : 0))
        {
            statement.Interpret();
        }


        if (inShell && _statements.Count > 0)
        {
            Console.WriteLine($"\x1B[95m> \x1B[93m{_statements.Last().Interpret(inShell)}\x1B[0m");
        }
    }
}
