using Errors;

namespace Parse.Statements;

public class Break : Statement
{
    private readonly int _line;

    public Break(int line)
    {
        _line = line;
    }
    
    public override void Interpret()
    {
        throw new BowBreak($"Unexpected break on line {_line}"); // Will be caught by loops and switches
    }
}
