namespace Tokenise;

public struct TokenType
{
    public const string
        LeftBracket      = "LEFT_BRACKET",
        RightBracket     = "RIGHT_BRACKET",
        
        Comma            = "COMMA",
        Seperator        = "SEPERATOR",
        
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
        
        NullReturn       = "NULL_RETURN",
        
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
        Alt              = "ALT",
        AltIf            = "ALT_IF",
        
        Case             = "CASE",
        Other            = "OTHER",
        Break            = "BREAK",
        CaseBranch       = "CASE_BRANCH",
        
        Fun              = "FUN",
        
        OpenBlock        = "OPEN_BLOCK",
        CloseBlock       = "CLOSE_BLOCK",
        FunTypeOpenBlock = "Fun_Type_Open_Block",
        
        ReturnArrow      = "RETURN_ARROW",
        
        EOF              = "EOF";
}
