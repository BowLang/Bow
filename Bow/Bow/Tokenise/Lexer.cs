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
                AddToken(TokenType.Minus);
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
            case ' ': case '\r': case '\t':
                break;
            case '\n':
                Console.WriteLine(_line);
                _line++;
                break;
            default:
                if (Char.IsDigit(C))
                {
                    Dec();
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
        AddToken(TokenType.Str, value);
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
        
        AddToken(TokenType.Dec, _code[_start.._current]);
    }
}
