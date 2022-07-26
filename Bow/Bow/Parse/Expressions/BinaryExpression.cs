﻿using System.Globalization;
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
        
        if (left.Type == TokenType.NullReturn || right.Type == TokenType.NullReturn)
        {
            throw new BowTypeError($"Cannot perform operation on non-returning function on line {_line}");
        }
        
        if (left.Type != right.Type)
        {
            throw new BowTypeError($"Cannot perform operation on two different types on line {_line}");
        }
        
        // Comparison Operators

        if (left.Type == TokenType.DecLiteral)
        {
            switch (_operator.Type)
            {
                case TokenType.LessThan:
                    return new BooLiteral(left.Value < right.Value);
                case TokenType.LessThanEqual:
                    return new BooLiteral(left.Value <= right.Value);
                case TokenType.GreaterThan:
                    return new BooLiteral(left.Value > right.Value);
                case TokenType.GreaterThanEqual:
                    return new BooLiteral(left.Value >= right.Value);
            }
        }
        
        switch (_operator.Type)
        {
            case TokenType.Equal:
                return new BooLiteral(left.Value == right.Value);
            case TokenType.NotEqual:
                return new BooLiteral(left.Value != right.Value);
        }
        
        // Arithmetic Operators
        
        switch (left.Type)
        {
            case TokenType.BooLiteral:
                return _operator.Type switch
                {
                    TokenType.And => new BooLiteral(left.Value && right.Value),
                    TokenType.Or => new BooLiteral(left.Value || right.Value),
                    _ => throw new BowTypeError($"Can't perform {_operator.Type} operation on booleans on line {_line}")
                };
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
