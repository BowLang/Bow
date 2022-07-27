using Errors;
using Tokenise;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.Literals;

namespace Parse.Statements;

public class Assignment : Statement
{
    private readonly string _name;
    private readonly Expression _valueExpression;
    private readonly int _line;
    
    public Assignment(string name, Expression valueExpression, int line)
    {
        _name  = name;
        _valueExpression = valueExpression;
        _line  = line;
    }

    public override void Interpret()
    {
        Literal value = _valueExpression.Evaluate();

        if (!Env.IsVariableDefined(_name))
        {
            throw new BowSyntaxError($"Unknown variable '{_name}' on line {_line}");
        }
        
        Symbol symbol = Env.GetVariable(_name);
        
        if (symbol.IsConstant)
        {
            throw new BowSyntaxError($"Cannot assign to constant '{_name}' on line {_line}");
        }
        
        Literal oldValue = symbol.Literal;

        if (oldValue.Type != value.Type)
        {
            throw new BowTypeError(
                $"Can't assign {value.Type} to variable of type {oldValue.Type[..3]} on line {_line}");
        }

        Literal newValue = oldValue.Type switch
        {
            TokenType.BooLiteral => new BooLiteral(value.Value),
            TokenType.DecLiteral => new DecLiteral(value.Value),
            TokenType.StrLiteral => new StrLiteral(value.Value),
            _ => throw new BowRuntimeError($"Current symbol type is incorrect on line {_line}")
        };

        symbol.SetValue(newValue);
    }

    public override string Interpret(bool lastInShell)
    {
        Interpret();
        
        return lastInShell ? Env.GetVariable(_name).Literal.DisplayValue : "";
    }
}
