namespace Tokenise; 

public class Token
{
    public string Type    { get; }
    public string Lexeme  { get; }
    public string Literal { get; }
    public int    Line    { get; }
    
    public Token(string type, string lexeme, string literal, int line)
    {
        Type    = type;
        Lexeme  = lexeme;
        Literal = literal;
        Line    = line;
    }

    public string Inspect()
    {
        return $"<Token Type=`{Type}` Lexeme=`{Lexeme}` Literal=`{Literal}` Line=`{Line}`>";
    }
}
