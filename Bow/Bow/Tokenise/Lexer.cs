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
            case '<':
                LessThan();
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

    private void LessThan()
    {
        if (Peek() == '-')
        {
            Advance();
            LeftSingleArrow();
        }
    }
    
    private void LeftSingleArrow()
    {
        if (PeekNext() == '<')
        {
            Assign();
        }
        else
        {
            Declare();
        }
    }

    private void Assign()
    {
        Advance();
        AddToken(TokenType.Assign);
    }
    
    private void Declare()
    {
        AddToken(TokenType.OpenDeclare);
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
            throw new BowSyntaxError($"unterminated string on line {_line}");
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
