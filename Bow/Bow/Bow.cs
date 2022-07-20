using Errors;
using Tokenise;

public class Bow
{
    private readonly string _code;
    
    public Bow(string fileName)
    {
        _code = String.Join("\n", File.ReadAllLines(fileName));
    }
    
    public void Run()
    {
        try
        {
            List<Token> tokens = new Lexer(_code).ScanTokens();
            foreach (var token in tokens)
            {
                Console.WriteLine(token.Inspect());
            }
        }
        catch (BowSyntaxError ex)
        {
            Console.WriteLine(ex);
        }
    }
}
