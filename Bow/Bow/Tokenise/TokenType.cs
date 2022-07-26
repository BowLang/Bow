namespace Tokenise;

public struct TokenType
{
    public const string
        Plus         = "+",
        Minus        = "-",
        Star         = "*",
        Slash        = "/",
        Dot          = ".",
        
        StrLiteral   = "STRLITERAL",
        DecLiteral   = "DECLITERAL",
        BooLiteral   = "BOOLITERAL",
        
        Str          = "STR",
        Dec          = "DEC",
        Boo          = "BOO",
        
        Var         = "VAR",
        Con         = "CON",
        
        Assign       = "ASSIGN",
        OpenDeclare  = "OPEN_DECLARE",
        CloseDeclare = "CLOSE_DECLARE",
        Identifier   = "IDENTIFIER",
        
        EOF          = "EOF";
}
