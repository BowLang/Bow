using Parse.Environment;
using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class VariableExpression : Expression
{
    private readonly string _name;
    private readonly int _line;
    
    public VariableExpression(string name, int line)
    {
        _name = name;
        _line = line;
    }

    public override Literal Evaluate()
    {
        Symbol var = Env.GetVariable(_name);
        
        return new Literal(var.Value, var.Type);
    }
}
