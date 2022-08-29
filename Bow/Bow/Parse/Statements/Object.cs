using Parse.Environment;

namespace Parse.Statements;

public class Object : Statement
{
    private readonly string _name;
    private readonly List<Statement> _statements;
    private readonly int _line;
    
    public Object(string name, List<Statement> statements, int line)
    {
        _name = name;
        _statements = statements;
        _line = line;
    }

    public override void Interpret()
    {
        ObjectSymbol symbol = Env.IsObjectDefined(_name, _line)
            ? Env.GetObject(_name, _line)
            : new ObjectSymbol(_name, null, _line);
        
        // populate the symbol with all the variables and methods
        Env.CurrentObj = symbol;
        
        Env.PushScope(new Env());
        
        foreach (Statement statement in _statements)
        {
            statement.Interpret();
        }
        
        Env.PopScope();

        Env.CurrentObj = null;

        Env.AddObject(symbol);
    }
}
