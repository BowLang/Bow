using System.Globalization;
using Errors;
using Tokenise;

using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class BinaryExpression : Expression
{
    private readonly Expression _left;
    private readonly Expression _right;
    private readonly Token _operator;
    private readonly int _line;
    
    public BinaryExpression(Expression left, Token op, Expression right, int line)
    {
        _left = left;
        _right = right;
        _operator = op;
        _line = line;
    }

    public override Literal Evaluate()
    {
        Literal left = _left.Evaluate();
        Literal right = _right.Evaluate();
        
        if (left.Type != right.Type)
        {
            throw new BowTypeError($"Cannot perform operation on two different types on line {_line}");
        }

        switch (left.Type)
        {
            case TokenType.BooLiteral:
                throw new BowTypeError($"Can't perform operations on booleans on line {_line}");
            case TokenType.StrLiteral:
                return _operator.Type switch
                {
                    TokenType.Plus => new StrLiteral(left.Value + right.Value),
                    _ => throw new BowTypeError(
                        $"Can't perform {_operator.Type} operation on strings on line {_line}")
                };
            case TokenType.DecLiteral:
                return _operator.Type switch
                {
                    TokenType.Plus  => new DecLiteral(left.Value + right.Value),
                    TokenType.Minus => new DecLiteral(left.Value - right.Value),
                    TokenType.Star  => new DecLiteral(left.Value * right.Value),
                    TokenType.Slash => new DecLiteral(left.Value / right.Value),
                    _ => throw new BowTypeError(
                        $"Can't perform `{_operator.Type} operation on decimals on line {_line}")
                };
            default:
                throw new BowTypeError($"Can't perform operations on {left.Type} on line {_line}");
        }
    } 
}
