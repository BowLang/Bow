using Errors;
using Tokenise;
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
        Literal value = Env.IsFunctionDefined(_name, _line)
            ? new FunctionExpression(_name, new(), _line).Evaluate()
            : Env.GetVariable(_name).Literal;

        return value.Type switch
        {
            TokenType.StrLiteral => new StrLiteral(value.Value),
            TokenType.BooLiteral => new BooLiteral(value.Value),
            TokenType.DecLiteral => new DecLiteral(value.Value),
            TokenType.NullReturn => new NullReturn(),
            _ => throw new BowRuntimeError($"Variable expression contains unknown type {value.Type} on line {_line}")
        };
    }
}
