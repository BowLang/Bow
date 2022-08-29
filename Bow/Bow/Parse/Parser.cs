using Errors;
using Tokenise;

using Parse.Statements;
using Parse.Expressions;

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

            return Match(new[] { TokenType.Assign })
                ? AssignStatement(name, Previous().Line)
                : LiteralStatement(Previous().Line);
        }

        if (Match(new[] { TokenType.Attribute }))
        {
            string name = Previous().Literal;

            return Match(new[] { TokenType.Assign })
                ? AssignStatement(name, Previous().Line, true)
                : LiteralStatement(Previous().Line);
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

        if (Match(new[] { TokenType.Break }))
        {
            return new Break(Previous().Line);
        }

        if (Match(new[] { TokenType.Switch }))
        {
            return SwitchStatement();
        }

        if (Match(new[] { TokenType.Obj }))
        {
            return ObjStatement();
        }

        if (Match(new[] { TokenType.Pub, TokenType.Pri }))
        {
            return ObjMethOrAttrDeclarationStatement();
        }

        if (Match(new[] { TokenType.DecLiteral, TokenType.BooLiteral, TokenType.StrLiteral, TokenType.LeftBracket }))
        {
            return LiteralStatement(Previous().Line);
        }

        Advance();
        throw new BowSyntaxError($"Unexpected token '{Previous().Lexeme}' on line {Previous().Line}");
    }

    private List<Statement> GetStatementBlock(string[] terminators)
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

        return statementList;
    }

    private List<Statement> GetCaseStatementBlock(int line)
    {
        List<Statement> statementList = new();

        while (!IsAtEnd() && !EndOfCaseBlock())
        {
            statementList.Add(GetStatement());
        }

        string[] terminators =
        {
            TokenType.CloseBlock, TokenType.Identifier, TokenType.BooLiteral, TokenType.StrLiteral,
            TokenType.DecLiteral, TokenType.Other
        };

        if (!terminators.Contains(Peek().Type))
        {
            throw new BowEOFError($"Unexpected EOF while looking for end of switch branch on line {line}");
        }

        if (statementList.Count == 0)
        {
            throw new BowSyntaxError($"Empty statement block on line {line}");
        }

        return statementList;
    }

    private bool EndOfCaseBlock()
    {
        if (Match(new[] { TokenType.CloseBlock }))
        {
            Undo();
            return true;
        }

        if (Match(new[]
            {
                TokenType.Identifier, TokenType.BooLiteral, TokenType.StrLiteral, TokenType.DecLiteral, TokenType.Other
            }))
        {
            if (Match(new[] { TokenType.CaseBranch }))
            {
                Undo();
                Undo();
                return true;
            }

            Undo();
            return false;
        }

        return false;
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
            Match(new[] { TokenType.Str, TokenType.Dec, TokenType.Boo, TokenType.Identifier }))
        {
            type = Previous().Literal;
        }
        else
        {
            throw new BowSyntaxError($"Declaration arrow invalid type on line {line}");
        }

        return (type, isConstant);
    }

    private Statement AssignStatement(string name, int line, bool isAttribute=false)
    {
        Expression valueExpression = GetExpression(Previous().Line);

        return new Assignment(name, valueExpression, isAttribute, line);
    }

    private Statement IfStatement(int line)
    {
        Expression ifCondition = GetExpression(Previous().Line);

        if (!Match(new[] { TokenType.OpenBlock }))
        {
            throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
        }

        List<Statement> ifStatements = GetStatementBlock(new[] { TokenType.CloseBlock });

        List<Tuple<Expression, List<Statement>>> altIfs = AltIfs();

        List<Statement> altStatements = Alt();

        return new If(ifCondition, ifStatements, altIfs, altStatements, line);
    }

    private List<Tuple<Expression, List<Statement>>> AltIfs()
    {
        List<Tuple<Expression, List<Statement>>> altIfs = new();

        while (Match(new[] { TokenType.AltIf }))
        {
            Expression altCondition = GetExpression(Previous().Line);

            if (!Match(new[] { TokenType.OpenBlock }))
            {
                throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
            }

            List<Statement> altIfStatements = GetStatementBlock(new[] { TokenType.CloseBlock });

            altIfs.Add(Tuple.Create(altCondition, altIfStatements));
        }

        return altIfs;
    }

    private List<Statement> Alt()
    {
        List<Statement> altStatements = new();

        if (Match(new[] { TokenType.Alt }))
        {
            if (!Match(new[] { TokenType.OpenBlock }))
            {
                throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
            }

            altStatements = GetStatementBlock(new[] { TokenType.CloseBlock });
        }

        return altStatements;
    }

    private Statement FunctionStatement(int line)
    {
        if (!Match(new[] { TokenType.Identifier }))
        {
            throw new BowSyntaxError($"Missing function name on line {line}");
        }

        string name = Previous().Literal;

        List<Tuple<string, List<string>>> parameters = GetParameters(line);

        List<string> returnTypes = GetReturnTypes();

        List<Statement> statements = GetStatementBlock(new[] { TokenType.CloseBlock });

        return new Function(name, parameters, returnTypes, statements, line);
    }

    private List<Tuple<string, List<string>>> GetParameters(int line)
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

    private List<string> GetReturnTypes()
    {
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

        return returnTypes;
    }

    private List<string> GetTypes(int line)
    {
        List<string> types = new() { GetType(line) };

        while (Match(new[] { TokenType.Separator }))
        {
            types.Add(GetType(line));
        }

        return types;
    }

    private string GetType(int line)
    {
        if (!Match(new[] { TokenType.Str, TokenType.Dec, TokenType.Boo, TokenType.Identifier }))
        {
            throw new BowTypeError($"Unknown type on line {line}");
        }

        return Previous().Literal;
    }

    private Statement ReturnStatement()
    {
        Expression returnExpression = GetExpression(Previous().Line, false);

        return new Return(returnExpression);
    }

    private Statement SwitchStatement()
    {
        Expression caseExpression = GetExpression(Previous().Line);

        if (!Match(new[] { TokenType.OpenBlock }))
        {
            throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
        }

        List<Tuple<List<Expression>, List<Statement>>> cases = GetCases(Previous().Line);
        List<Statement> other = GetOther(Previous().Line);

        if (!Match(new[] { TokenType.CloseBlock }))
        {
            throw new BowEOFError($"Missing '<==' on line {Previous().Line}");
        }

        return new Switch(caseExpression, cases, other, Previous().Line);
    }

    private List<Tuple<List<Expression>, List<Statement>>> GetCases(int line)
    {
        List<Tuple<List<Expression>, List<Statement>>> cases = new();

        while (Peek().Type != TokenType.Other && Peek().Type != TokenType.CloseBlock)
        {
            List<Expression> caseExpressions = new() { GetExpression(line) };

            while (Match(new[] { TokenType.Separator }))
            {
                caseExpressions.Add(GetExpression(line));
            }

            if (!Match(new[] { TokenType.CaseBranch }))
            {
                throw new BowSyntaxError($"Missing case branch arrow on line {line}");
            }

            List<Statement> statements = GetCaseStatementBlock(line);

            cases.Add(Tuple.Create(caseExpressions, statements));
        }

        return cases;
    }

    private List<Statement> GetOther(int line)
    {
        if (!Match(new[] { TokenType.Other }))
        {
            return new();
        }

        if (!Match(new[] { TokenType.CaseBranch }))
        {
            throw new BowSyntaxError($"Missing case branch arrow on line {line}");
        }

        List<Statement> statements = GetStatementBlock(new[] { TokenType.CloseBlock });
        Undo(); // Get back the <== for checking
        return statements;
    }

    private Statement ObjStatement()
    {
        if (!Match(new[] { TokenType.Identifier, TokenType.Str, TokenType.Dec, TokenType.Boo }))
        {
            throw new BowSyntaxError($"Missing object name on line {Previous().Line}");
        }

        string name = Previous().Literal;

        if (!Match(new[] { TokenType.OpenBlock }))
        {
            throw new BowSyntaxError($"Missing '==>' on line {Peek().Line}");
        }

        List<Statement> statements = GetStatementBlock(new[] { TokenType.CloseBlock });
        
        return new Statements.Object(name, statements, Previous().Line);
    }

    private Statement ObjMethOrAttrDeclarationStatement()
    {
        bool isPrivate = Previous().Type == TokenType.Pri;
        bool isStatic = Match(new[] { TokenType.Stat });

        return Match(new[] { TokenType.Meth })
            ? MethStatement(isPrivate, isStatic)
            : AttributeDeclarationStatement(isPrivate, isStatic);
    }

    private Statement MethStatement(bool isPrivate, bool isStatic)
    {
        if (!Match(new[]
            {
                TokenType.Identifier, TokenType.Equal, TokenType.NotEqual, TokenType.LessThan, TokenType.LessThanEqual,
                TokenType.GreaterThan, TokenType.GreaterThanEqual, TokenType.Plus, TokenType.Minus, TokenType.Star,
                TokenType.Slash, TokenType.And, TokenType.Or, TokenType.Not, TokenType.Negate
            }))
        {
            throw new BowSyntaxError($"Missing method name on line {Previous().Line}");
        }

        string name = Previous().Literal;

        if (name == "")
        {
            name = Previous().Lexeme;
        }

        List<Tuple<string, List<string>>> parameters = GetParameters(Previous().Line);

        List<string> returnTypes = GetReturnTypes();

        List<Statement> statements = GetStatementBlock(new[] { TokenType.CloseBlock });

        return new Method(isPrivate, name, parameters, returnTypes, statements, isStatic, Previous().Line);
    }

    private Statement AttributeDeclarationStatement(bool isPrivate, bool isStatic)
    {
        if (!Match(new[] { TokenType.Attribute }))
        {
            throw new BowSyntaxError($"Missing variable name on line {Previous().Line}");
        }

        string name = Previous().Literal;
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
            throw new BowSyntaxError($"Missing var/con on line {Peek().Line}");
        }

        string type = GetType(Previous().Line);

        return new AttributeDeclaration(isPrivate, name, type, isConstant, isStatic, Previous().Line);
    }

    private Statement LiteralStatement(int line)
    {
        Undo();

        Expression valueExpression = GetExpression(line);

        return new LitStatement(valueExpression, line);
    }

    // Expressions

    private Expression GetExpression(int line, bool checkNone = true)
    {
        Expression expression = GetOr(line);

        if (checkNone && expression == null)
        {
            throw new BowSyntaxError($"missing expression on line {line}");
        }

        return expression;
    }

    private string TokenToBinOp(Token token, int line)
    {
        return token.Type switch
        {
            TokenType.Equal            => "=",
            TokenType.NotEqual         => "!=",
            TokenType.LessThan         => "<",
            TokenType.LessThanEqual    => "<=",
            TokenType.GreaterThan      => ">",
            TokenType.GreaterThanEqual => ">=",
            TokenType.Plus             => "+",
            TokenType.Minus            => "-",
            TokenType.Star             => "*",
            TokenType.Slash            => "/",
            TokenType.And              => "&&",
            TokenType.Or               => "||",
            _ => throw new BowRuntimeError($"Unrecognised binary operator on line {line}")
        };
    }

    private string TokenToUnaryOp(Token token, int line)
    {
        return token.Type switch
        {
            TokenType.Minus => "-@",
            TokenType.Not   => "!",
            _ => throw new BowTypeError($"Unrecognised unary operator on line {line}")
        };
    }

    private Expression GetOr(int line)
    {
        Expression expression = GetAnd(line);

        while (Match(new[] { TokenType.Or }))
        {
            string op = TokenToBinOp(Previous(), line);
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
            string op = TokenToBinOp(Previous(), line);
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
            string op = TokenToBinOp(Previous(), line);
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
            string op = TokenToBinOp(Previous(), line);
            Expression right = GetFactor(line);
            expression = new BinaryExpression(expression, op, right, line);
        }

        return expression;
    }

    private Expression GetFactor(int line)
    {
        Expression expression = GetUnary(line);

        while (Match(new[] { TokenType.Star, TokenType.Slash }))
        {
            string op = TokenToBinOp(Previous(), line);
            Expression right = GetUnary(line);
            expression = new BinaryExpression(expression, op, right, line);
        }

        return expression;
    }

    private Expression GetUnary(int line)
    {
        if (Match(new[] { TokenType.Minus }))
        {
            string op = TokenToUnaryOp(Previous(), line);
            Expression right = GetUnary(line);
            return new UnaryExpression(op, right, line);
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
            return new BuiltinObjectExpression("boo", Previous().Literal, line);
        }

        if (Match(new[] { TokenType.DecLiteral }))
        {
            return new BuiltinObjectExpression("dec", Previous().Literal, line);
        }

        if (Match(new[] { TokenType.StrLiteral }))
        {
            return new BuiltinObjectExpression("str", Previous().Literal, line);
        }

        if (Match(new[] { TokenType.Identifier, TokenType.Attribute }))
        {
            string name = Previous().Literal;
            Expression expression;

            if (Previous().Type == TokenType.Attribute)
            {
                expression = new AttributeExpression(name, null, line);
            }
            else if (Peek().Type == TokenType.LeftBracket)
            {
                expression = GetFunction(name, line);
            }
            else if (Match(new[] { TokenType.DoubleColon }))
            {
                if (!Match(new[] { TokenType.ObjAccessor }))
                {
                    throw new BowSyntaxError($"Missing method after '::' on line {Previous().Line}");
                }

                if (Previous().Literal == "new")
                {
                    expression = new ObjectExpression(name, GetFunctionParameters(line), Previous().Line);
                }
                else
                {
                    Undo();
                    Undo();
                    expression = new VariableExpression(name, Previous().Line);
                }
            }
            else
            {
                expression = new VariableExpression(name, Previous().Line);
            }
            

            if (Peek().Type is TokenType.Colon or TokenType.DoubleColon)
            {
                while (Peek().Type is TokenType.Colon or TokenType.DoubleColon)
                {
                    if (Match(new[] { TokenType.Colon }))
                    {
                        if (!Match(new[] { TokenType.ObjAccessor }))
                        {
                            throw new BowSyntaxError(
                                $"Expected identifier after attribute access colon on line {Previous().Line}");
                        }

                        expression = new AttributeExpression($"#{Previous().Literal}", expression, Previous().Line);
                    }
                    else if (Match(new[] { TokenType.DoubleColon }))
                    {
                        if (!Match(new[] { TokenType.ObjAccessor }))
                        {
                            throw new BowSyntaxError(
                                $"Expected identifier after method access colon on line {Previous().Line}");
                        }

                        expression = new MethodExpression(Previous().Literal, GetFunctionParameters(line), expression, 
                            line);
                    }
                }

                return expression;
            }

            return expression;
        }

        throw new BowSyntaxError($"Unexpected token '{Peek().Lexeme}' on line {Peek().Line}");
    }

    private FunctionExpression GetFunction(string name, int line)
    {
        return new FunctionExpression(name, GetFunctionParameters(line), Previous().Line);
    }

    private List<Expression> GetFunctionParameters(int line)
    {
        List<Expression> parameters = new();

        if (Match(new[] { TokenType.LeftBracket }))
        {
            if (Peek().Type != TokenType.RightBracket)
            {
                Expression e = GetExpression(line);
                parameters.Add(e);

                while (Match(new[] { TokenType.Comma }))
                {
                    if (IsAtEnd())
                    {
                        throw new BowEOFError($"Unexpected EOF when looking for parameters on line {line}");
                    }

                    parameters.Add(GetExpression(line));
                }
            }

            if (!Match(new[] { TokenType.RightBracket }))
            {
                throw new BowSyntaxError($"Missing ')' on line {line}");
            }
        }

        return parameters;
    }
}
