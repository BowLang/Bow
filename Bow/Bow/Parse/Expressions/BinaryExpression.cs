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
            throw new BowSyntaxError($"Cannot perform operation on two different types on line {_line}");
        }

        switch (left.Type)
        {
            case TokenType.BooLiteral:
                throw new BowSyntaxError($"Can't perform operations on booleans on line {_line}");
            case TokenType.StrLiteral:
                return _operator.Type switch
                {
                    TokenType.Plus => new Literal(left.Value + right.Value, TokenType.StrLiteral),
                    _ => throw new BowSyntaxError(
                        $"Can't perform {_operator.Type} operation on strings on line {_line}")
                };
            case TokenType.DecLiteral:
                double leftDec = double.Parse(left.Value);
                double rightDec = double.Parse(right.Value);

                return _operator.Type switch
                {
                    TokenType.Plus => new Literal((leftDec + rightDec).ToString(CultureInfo.CurrentCulture),
                        TokenType.DecLiteral),
                    TokenType.Minus => new Literal((leftDec - rightDec).ToString(CultureInfo.CurrentCulture),
                        TokenType.DecLiteral),
                    TokenType.Star => new Literal((leftDec * rightDec).ToString(CultureInfo.CurrentCulture),
                        TokenType.DecLiteral),
                    TokenType.Slash => new Literal((leftDec / rightDec).ToString(CultureInfo.CurrentCulture),
                        TokenType.DecLiteral),
                    _ => throw new BowSyntaxError(
                        $"Can't perform `{_operator.Type} operation on decimals on line {_line}")
                };
            default:
                throw new BowSyntaxError($"Can't perform operations on {left.Type} on line {_line}");
        }
    } 
}
