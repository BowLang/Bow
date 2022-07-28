namespace Tokenise;

public struct TokenType
{
    public const string
        Plus             = "PLUS",
        Minus            = "MINUS",
        Star             = "STAR",
        Slash            = "SLASH",
        Dot              = "DOT",
        
        Equal            = "EQUAL",
        NotEqual         = "NOT_EQUAL",
        LessThan         = "LESS_THAN",
        LessThanEqual    = "LESS_THAN_EQUAL",
        GreaterThan      = "GREATER_THAN",
        GreaterThanEqual = "GREATER_THAN_EQUAL",
        
        Not              = "NOT",
        And              = "AND",
        Or               = "OR",
        
        StrLiteral       = "STRLITERAL",
        DecLiteral       = "DECLITERAL",
        BooLiteral       = "BOOLITERAL",
        
        Str              = "STR",
        Dec              = "DEC",
        Boo              = "BOO",
        
        Var              = "VAR",
        Con              = "CON",
        
        Assign           = "ASSIGN",
        OpenDeclare      = "OPEN_DECLARE",
        CloseDeclare     = "CLOSE_DECLARE",
        Identifier       = "IDENTIFIER",
        
        If               = "IF",
        
        OpenBlock        = "OPEN_BLOCK",
        CloseBlock       = "CLOSE_BLOCK",
        
        EOF              = "EOF";
}
