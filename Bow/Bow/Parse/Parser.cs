using Errors;
using Tokenise;

using Parse.Statements;
using Parse.Expressions;
using Parse.Expressions.Literals;

namespace Parse;

public class Parser
{
    private int _current;
    private readonly List<Token> _tokens;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public List<Statement> Parse()
    {
        List<Statement> statements = new();

        while (!IsAtEnd())
        {
            statements.Add(GetStatement());
        }

        return statements;
    }

    private bool Match(string[] tokens)
    {
        if (tokens.Contains(Peek().Type))
        {
            Advance();
            return true;
        }

        return false;
    }

    private Token Peek()
    {
        return _tokens[_current];
    }

    private Token Advance()
    {
        if (_current < _tokens.Count)
        {
            Token token = _tokens[_current];
            _current++;
            return token;
        }

        throw new BowSyntaxError($"Unexpected EOF at line {_tokens[_current].Line}");
    }

    private Token Previous()
    {
        if (_current > 0)
        {
            return _tokens[_current - 1];
        }

        throw new BowRuntimeError("Cannot call Previous() on the first token");
    }

    private void Undo()
    {
        if (_current > 0)
        {
            _current--;
        }
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    // Statements

    private Statement GetStatement()
    {
        if (Match(new[] { TokenType.Identifier }))
        {
            string name = Previous().Literal;

            if (Match(new[] { TokenType.OpenDeclare }))
            {
                return DeclareStatement(name, Previous().Line);
            }

            return LiteralStatement(Previous().Line);
        }
        
        if (Match(new[] { TokenType.DecLiteral, TokenType.BooLiteral, TokenType.StrLiteral }))
        {
            return LiteralStatement(Previous().Line);
        }

        Advance();
        throw new BowSyntaxError($"Unexpected token '{Previous().Lexeme}' on line {Previous().Line}");
    }

    private Statement DeclareStatement(string name, int line)
    {
        var (type, isConstant) = DeclareType(line);

        if (!Match(new[] { TokenType.CloseDeclare }))
        {
            throw new BowSyntaxError($"Missing end of declaration arrow on line {line}");
        }
        
        Expression valueExpression = GetExpression(Previous().Line);

        return new Declaration(name, valueExpression, type, isConstant, line);
    }

    private (string, bool) DeclareType(int line)
    {
        string type;
        bool isConstant;

        if (Match(new[] { TokenType.Var }))
        {
            isConstant = false;
        }
        else if (Match(new[] { TokenType.Con }))
        {
            isConstant = true;
        }
        else
        {
            throw new BowSyntaxError($"Declaration arrow missing 'var' or 'con' on line {line}");
        }

        if (
            Match(new[] { TokenType.Str, TokenType.Dec, TokenType.Boo }))
        {
            type = Previous().Type + "LITERAL";
        }
        else
        {
            throw new BowSyntaxError($"Declaration arrow invalid type on line {line}");
        }

        return (type, isConstant);
    }
    
    private Statement LiteralStatement(int line)
    {
        Undo();
        
        Expression valueExpression = GetExpression(line);

        return new LitStatement(valueExpression, line);
    }

    // Expressions

    private Expression GetExpression(int line, bool checkNone=true)
    {
        Expression expression = GetTerm(line);

        if (checkNone && expression == null)
        {
            throw new BowSyntaxError($"missing expression on line {line}");
        }

        return expression;
    }

    private Expression GetTerm(int line)
    {
        Expression expression = GetFactor(line);
        
        while (Match(new[] { TokenType.Plus, TokenType.Minus }))
        {
            Token op = Previous();
            Expression right = GetFactor(line);
            expression = new BinaryExpression(expression, op, right, op.Line);
        }
        
        return expression;
    }

    private Expression GetFactor(int line)
    {
        Expression expression = GetUnary(line);
        
        while (Match(new[] { TokenType.Star, TokenType.Slash }))
        {
            Token op = Previous();
            Expression right = GetUnary(line);
            expression = new BinaryExpression(expression, op, right, op.Line);
        }
        
        return expression;
    }

    private Expression GetUnary(int line)
    {
        if (Match(new[] { TokenType.Minus }))
        {
            Token op = Previous();
            Expression right = GetUnary(line);
            return new UnaryExpression(op, right, op.Line);
        }
        
        return Primary(line);
    }

    private Expression Primary(int line)
    {
        if (Match(new[] { TokenType.BooLiteral }))
        {
            return new LiteralExpression(new BooLiteral(Previous().Literal), line);
        }
        
        if (Match(new[] { TokenType.DecLiteral }))
        {
            return new LiteralExpression(new DecLiteral(Previous().Literal), line);
        }
        
        if (Match(new[] { TokenType.StrLiteral }))
        {
            return new LiteralExpression(new StrLiteral(Previous().Literal), line);
        }
        
        if (Match(new[] { TokenType.Identifier }))
        {
            return new VariableExpression(Previous().Literal, Previous().Line);
        }
        
        throw new BowSyntaxError($"Unexpected token '{Previous().Lexeme}' on line {Previous().Line}");
    }
}
