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

        throw new BowEOFError($"Unexpected EOF at line {_tokens[_current].Line}");
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
            
            if (Match(new[] { TokenType.Assign }))
            {
                return AssignStatement(name, Previous().Line);
            }

            return LiteralStatement(Previous().Line);
        }

        if (Match(new[] { TokenType.If }))
        {
            return IfStatement(Previous().Line);
        }

        if (Match(new[] { TokenType.Fun }))
        {
            return FunctionStatement(Previous().Line);
        }
        
        if (Match(new[] { TokenType.ReturnArrow }))
        {
            return ReturnStatement();
        }
        
        if (Match(new[] { TokenType.DecLiteral, TokenType.BooLiteral, TokenType.StrLiteral, TokenType.LeftBracket }))
        {
            return LiteralStatement(Previous().Line);
        }

        Advance();
        throw new BowSyntaxError($"Unexpected token '{Previous().Lexeme}' on line {Previous().Line}");
    }
    
    private List<Statement> GetStatementBlock(string[] terminators, int line)
    {
        List<Statement> statementList = new();

        while (!IsAtEnd() && !Match(terminators))
        {
            statementList.Add(GetStatement());
        }

        if (!terminators.Contains(Previous().Type))
        {
            throw new BowEOFError($"Unexpected EOF while looking for '{string.Join(", ", terminators)}'");
        }

        if (statementList.Count == 0)
        {
            throw new BowSyntaxError($"Empty statement block on line {line}");
        }

        return statementList;
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
    
    private Statement AssignStatement(string name, int line)
    {
        Expression valueExpression = GetExpression(Previous().Line);
        
        return new Assignment(name, valueExpression, line);
    }

    private Statement IfStatement(int line)
    {
        Expression ifCondition = GetExpression(Previous().Line);
        
        if (!Match(new[] { TokenType.OpenBlock }))
        {
            throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
        }
        
        List<Statement> ifStatements = GetStatementBlock(new[] { TokenType.CloseBlock }, line);

        List<Tuple<Expression, List<Statement>>> altIfs = AltIfs(line);
        
        List<Statement> altStatements = Alt(line);

        return new If(ifCondition, ifStatements, altIfs, altStatements, line);
    }

    private List<Tuple<Expression, List<Statement>>> AltIfs(int line)
    {
        List<Tuple<Expression, List<Statement>>> altIfs = new();
        
        while (Match(new[] { TokenType.AltIf }))
        {
            Expression altCondition = GetExpression(Previous().Line);
            
            if (!Match(new[] { TokenType.OpenBlock }))
            {
                throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
            }
            
            List<Statement> altIfStatements = GetStatementBlock(new[] { TokenType.CloseBlock }, line);
            
            altIfs.Add(Tuple.Create(altCondition, altIfStatements));
        }

        return altIfs;
    }

    private List<Statement> Alt(int line)
    {
        List<Statement> altStatements = new();

        if (Match(new[] { TokenType.Alt }))
        {
            if (!Match(new[] { TokenType.OpenBlock }))
            {
                throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
            }
            
            altStatements = GetStatementBlock(new[] { TokenType.CloseBlock }, line);
        }

        return altStatements;
    }
    
    private Statement FunctionStatement(int line)
    {
        if (!Match(new [] { TokenType.Identifier }))
        {
            throw new BowSyntaxError($"Missing function name on line {line}");
        }
        
        string name = Previous().Literal;
        
        List<Tuple<string, List<string>>> parameters = FunParameters(line);

        List<string> returnTypes = new();
        
        if (!Match(new[] { TokenType.OpenBlock }))
        {
            if (Match(new[] { TokenType.Equal }))
            {
                returnTypes = GetTypes(Previous().Line);

                if (!Match(new[] { TokenType.FunTypeOpenBlock }))
                {
                    throw new BowSyntaxError($"Missing end of function type return arrow on line {Previous().Line}");
                }
            }
        }
        
        List<Statement> statements = GetStatementBlock(new[] { TokenType.CloseBlock }, line);
        
        return new Function(name, parameters, returnTypes, statements, line);
    }

    private List<Tuple<string, List<string>>> FunParameters(int line)
    {
        List<Tuple<string, List<string>>> parameters = new();

        if (!Match(new[] { TokenType.LeftBracket })) return parameters;
        
        if (!Match(new[] { TokenType.RightBracket }))
        {
            if (IsAtEnd())
            {
                throw new BowEOFError($"Unexpected EOF when looking for parameters on line {line}");
            }

            parameters.Add(GetParameter(line));
        }
        
        while (Match(new[] { TokenType.Comma }))
        {
            if (IsAtEnd())
            {
                throw new BowEOFError($"Unexpected EOF when looking for parameters on line {line}");
            }
            
            parameters.Add(GetParameter(line));
        }
        
        if (!Match(new[] { TokenType.RightBracket }))
        {
            throw new BowSyntaxError($"Missing ')' on line {line}");
        }

        return parameters;
    }

    private Tuple<string, List<string>> GetParameter(int line)
    {
        if (!Match(new[] { TokenType.Identifier }))
        {
            throw new BowSyntaxError($"Missing parameter name on line {line}");
        }
                
        string name = Previous().Literal;
                
        if (!Match(new[] { TokenType.Minus }))
        {
            throw new BowSyntaxError($"Missing opening type '-' on line {line}");
        }

        List<string> types = GetTypes(line);
                
        if (!Match(new[] { TokenType.Minus }))
        {
            throw new BowSyntaxError($"Missing closing type '-' on line {line}");
        }

        return Tuple.Create(name, types);
    }

    private List<string> GetTypes(int line)
    {
        List<string> types = new();
                
        if (!Match(new[] { TokenType.Str, TokenType.Dec, TokenType.Boo }))
        {
            throw new BowTypeError($"Unknown type on line {line}");
        }
                
        types.Add(Previous().Type + "LITERAL");

        while (Match(new[] { TokenType.Seperator }))
        {
            if (!Match(new[] { TokenType.Str, TokenType.Dec, TokenType.Boo }))
            {
                throw new BowTypeError($"Unknown type on line {line}");
            }
                    
            types.Add(Previous().Type + "LITERAL");
        }

        return types;
    }

    private Statement ReturnStatement()
    {
        Expression returnExpression = GetExpression(Previous().Line, false);
        
        return new Return(returnExpression);
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
        Expression expression = GetOr(line);

        if (checkNone && expression == null)
        {
            throw new BowSyntaxError($"missing expression on line {line}");
        }

        return expression;
    }

    private Expression GetOr(int line)
    {
        Expression expression = GetAnd(line);
        
        while (Match(new[] { TokenType.Or }))
        {
            Token op = Previous();
            Expression right = GetAnd(line);
            expression = new BinaryExpression(expression, op, right, line);
        }

        return expression;
    }

    private Expression GetAnd(int line)
    {
        Expression expression = GetComparison(line);
        
        while (Match(new[] { TokenType.And }))
        {
            Token op = Previous();
            Expression right = GetComparison(line);
            expression = new BinaryExpression(expression, op, right, line);
        }

        return expression;
    }

    private Expression GetComparison(int line)
    {
        Expression expression = GetTerm(line);
        
        while (Match(new[]
               {
                   TokenType.Equal, TokenType.NotEqual, TokenType.LessThan, TokenType.LessThanEqual,
                   TokenType.GreaterThan, TokenType.GreaterThanEqual
               }))
        {
            Token op = Previous();
            Expression right = GetTerm(line);
            expression = new BinaryExpression(expression, op, right, line);
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
        if (IsAtEnd())
        {
            throw new BowEOFError($"Unexpected EOF on line {Peek().Line}");
        }
        
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
        
        if (Match(new[] { TokenType.LeftBracket }))
        {
            Expression expression = GetExpression(line);
            
            if (!Match(new[] { TokenType.RightBracket }))
            {
                throw new BowEOFError($"Missing ')' on line {Peek().Line}");
            }
            
            return expression;
        }
        
        throw new BowSyntaxError($"Unexpected token '{Previous().Lexeme}' on line {Previous().Line}");
    }
}
