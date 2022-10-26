using Parse.Environment;

namespace Parse.Statements;

public class Object : Statement
{
    private readonly string _name;
    private readonly List<Statement> _statements;
    private readonly string? _super;
    private readonly int _line;
    
    public Object(string name, List<Statement> statements, string? super, int line)
    {
        _name = name;
        _statements = statements;
        _super = super;
        _line = line;
    }

    public override void Interpret()
    {
        ObjectSymbol? super = _super is null ? null : Env.GetObject(_super, _line);
        
        ObjectSymbol symbol = Env.IsObjectDefined(_name, _line)
            ? Env.GetObject(_name, _line)
            : new ObjectSymbol(_name, super, _line);

        super?.AddChild(symbol);

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
