﻿using System.Globalization;
using Errors;
using Tokenise;
using Parse.Expressions.Literals;

namespace Parse.Expressions;

public class UnaryExpression : Expression
{
    private readonly Expression _right;
    private readonly Token _operator;
    private readonly int _line;
    
    public UnaryExpression(Token op, Expression right, int line)
    {
        _right = right;
        _operator = op;
        _line = line;
    }

    public override Literal Evaluate()
    {
        Literal right = _right.Evaluate();

        if (right.Type == TokenType.NullReturn)
        {
            throw new BowTypeError($"Cannot perform operation on non-returning function on line {_line}");
        }
        
        switch (_operator.Type)
        {
            case TokenType.Minus:
                if (right.Type != TokenType.DecLiteral)
                {
                    throw new BowTypeError($"Can't negate non-decimal value on line {_line}");
                }
                return new DecLiteral(-right.Value);
            default:
                throw new BowTypeError($"Can't perform `{_operator.Type} operation {right.Type} on line {_line}");
        }
    }
}
