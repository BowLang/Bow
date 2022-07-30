namespace Tokenise;

public struct Keywords
{
    private static readonly Dictionary<string, string> Words = new()
    {
        { "var",   TokenType.Var        },
        { "con",   TokenType.Con        },
        { "str",   TokenType.Str        },
        { "dec",   TokenType.Dec        },
        { "boo",   TokenType.Boo        },
        { "true",  TokenType.BooLiteral },
        { "false", TokenType.BooLiteral },
        { "if",    TokenType.If         },
        { "alt",   TokenType.Alt        },
        { "altif", TokenType.AltIf      }
    };

    public static bool Contains(string key)
    {
        return Words.ContainsKey(key);
    }

    public static string GetTokenType(string key)
    {
        return Words[key];
    }
}
