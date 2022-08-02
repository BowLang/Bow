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
        char C = Advance();
        
        switch (C)
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
            case ' ': case '\r': case '\t':
                break;
            case '\n':
                _line++;
                break;
            default:
                if (Char.IsDigit(C))
                {
                    Dec();
                }
                else if (Char.IsLetter(C))
                {
                    Identifier();
                }
                else
                {
                    throw new BowSyntaxError($"Unrecognised character {C} on line {_line}");
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
        char C = _code[_current];
        _current++;

        return C;
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

        if (Peek() == '-')
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
            AddToken(TokenType.Seperator);
        }
        else
        {
            Advance();
            AddToken(TokenType.Or);
        }
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

        if (Keywords.Contains(identifier))
        {
            AddToken(Keywords.GetTokenType(identifier), identifier);
        }
        else
        {
            AddToken(TokenType.Identifier, identifier);
        }
    }
}
