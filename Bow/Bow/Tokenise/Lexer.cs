using Errors;

namespace Tokenise;

public class Lexer
{
    private int _start;
    private int _current;
    private int _line = 1;
    private readonly string _code;
    private readonly List<Token> _tokens = new();

    public Lexer(string code)
    {
        _code = code;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }
        
        _tokens.Add(new Token(TokenType.EOF, "", "\0", _line));
        
        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        
        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftBracket);
                break;
            case ')':
                AddToken(TokenType.RightBracket);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case '-':
                Minus();
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '/':
                AddToken(TokenType.Slash);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '\'': case '"':
                Str();
                break;
            case '>':
                GreaterThan();
                break;
            case '<':
                LessThan();
                break;
            case '=':
                Equals();
                break;
            case '!':
                Bang();
                break;
            case '&':
                And();
                break;
            case '|':
                Or();
                break;
            case '?':
                Question();
                break;
            case '#':
                Hash();
                break;
            case ':':
                Colon();
                break;
            case ' ': case '\r': case '\t':
                break;
            case '\n':
                _line++;
                break;
            default:
                if (Char.IsDigit(c))
                {
                    Dec();
                }
                else if (Char.IsLetter(c) || c == '_')
                {
                    Identifier();
                }
                else
                {
                    throw new BowSyntaxError($"Unrecognised character {c} on line {_line}");
                }
                break;
        }
    }

    private void AddToken(string type, string literal="")
    {
        _tokens.Add(new Token(type, _code[_start.._current], literal, _line));
    }

    private char Advance()
    {
        char c = _code[_current];
        _current++;

        return c;
    }

    private bool IsAtEnd()
    {
        return _current >= _code.Length;
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : _code[_current];
    }

    private char PeekNext()
    {
        return _current + 1 > _code.Length ? '\0' : _code[_current + 1];
    }

    private void Minus()
    {
        if (Peek() == '<')
        {
            Advance();
            AddToken(TokenType.CloseDeclare);
        }
        else if (Peek() == '@')
        {
            AddToken(TokenType.Negate);
        }
        else
        {
            AddToken(TokenType.Minus);
        }
    }
    
    private void GreaterThan()
    {
        if (Peek() == '=')
        {
            Advance();
            AddToken(TokenType.GreaterThanEqual);
        }
        else
        {
            AddToken(TokenType.GreaterThan);
        }
    }

    private void LessThan()
    {
        if (Peek() == '-')
        {
            Advance();
            LeftSingleArrow();
        }
        else if (Peek() == '=')
        {
            Advance();
            LessThanEqual();
        }
        else
        {
            AddToken(TokenType.LessThan);
        }
    }
    
    private void LeftSingleArrow()
    {
        if (Peek() == '<')
        {
            Advance();
            AddToken(TokenType.Assign);
        }
        else if (Peek() == '-')
        {
            Advance();
            AddToken(TokenType.ReturnArrow);
        }
        else
        {
            AddToken(TokenType.OpenDeclare);
        }
    }
    
    private void LessThanEqual()
    {
        if (Peek() == '=')
        {
            Advance();
            AddToken(TokenType.CloseBlock);
        }
        else
        {
            AddToken(TokenType.LessThanEqual);
        }
    }

    private void Equals()
    {
        if (Peek() == '=')
        {
            Advance();
            RightDoubleArrow();
        }
        else if (Peek() == '>')
        {
            Advance();
            AddToken(TokenType.FunTypeOpenBlock);
        }
        else
        {
            AddToken(TokenType.Equal);
        }
    }

    private void RightDoubleArrow()
    {
        if (Peek() != '>')
        {
            throw new BowSyntaxError($"Malformed open block arrow on line {_line}");
        }

        Advance();
        AddToken(TokenType.OpenBlock);
    }

    private void Bang()
    {
        if (Peek() == '=')
        {
            Advance();
            AddToken(TokenType.NotEqual);
        }
        else
        {
            AddToken(TokenType.Not);
        }
    }

    private void And()
    {
        if (Peek() != '&')
        {
            throw new BowSyntaxError($"Expected '&' on line {_line}");
        }
        
        Advance();
        AddToken(TokenType.And);
    }

    private void Or()
    {
        if (Peek() != '|')
        {
            AddToken(TokenType.Separator);
        }
        else
        {
            Advance();
            AddToken(TokenType.Or);
        }
    }
    
    private void Question()
    {
        if (Peek() != '-')
        {
            throw new BowSyntaxError($"'Malformed case branch arrow on line {_line}");
        }
        
        Advance();
        CaseBranch();
    }

    private void CaseBranch()
    {
        if (Peek() != '>')
        {
            throw new BowSyntaxError($"'Malformed case branch arrow on line {_line}");
        }
        
        Advance();
        AddToken(TokenType.CaseBranch);
    }

    private void Hash()
    {
        char c = Peek();
        if (!Char.IsLetter(c) && c != '_')
        {
            throw new BowSyntaxError($"Unexpected # on line {_line}");
        }

        InstanceVariable();
    }

    private void InstanceVariable()
    {
        if (Char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            Advance();
            
            while (Char.IsLetterOrDigit(Peek()) || Peek() == '_')
            {
                Advance();
            }
        }
        
        string identifier = _code[_start.._current];
        
        AddToken(TokenType.Attribute, identifier);
    }

    private void Colon()
    {
        if (Peek() == ':')
        {
            Advance();
            AddToken(TokenType.DoubleColon);
            _start++;
        }
        else
        {
            AddToken(TokenType.Colon);
        }

        _start++;

        ObjectAccess();
    }

    private void ObjectAccess()
    {
        if (Char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            Advance();
            
            while (Char.IsLetterOrDigit(Peek()) || Peek() == '_')
            {
                Advance();
            }
        }

        string identifier = _code[_start.._current];
        
        AddToken(TokenType.ObjAccessor, identifier);
    }

    private void Str()
    {
        char type = _code[_current - 1];
        
        while (!IsAtEnd() && Peek() != type)
        {
            if (Peek() == '\n')
            {
                _line++;
            }

            Advance();
        }

        if (IsAtEnd())
        {
            throw new BowEOFError($"unterminated string on line {_line}");
        }

        Advance();

        string value = _code[(_start + 1)..(_current - 1)];
        AddToken(TokenType.StrLiteral, value);
    }

    private void Dec()
    {
        while (Char.IsDigit(Peek()))
        {
            Advance();
        }
        
        if (Peek() == '.' && Char.IsDigit(PeekNext()))
        {
            Advance();

            while (Char.IsDigit(Peek()))
            {
                Advance();
            }
        }
        
        AddToken(TokenType.DecLiteral, _code[_start.._current]);
    }

    private void Identifier()
    {
        while (Char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            Advance();
        }

        string identifier = _code[_start.._current];

        AddToken(Keywords.Contains(identifier) ? Keywords.GetTokenType(identifier) : TokenType.Identifier, identifier);
    }
}
