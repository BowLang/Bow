using Parse;
using Parse.Statements;
using Parse.Environment;

using Interpret;

using Errors;
using Tokenise;

public class Bow
{
    private readonly string _code;
    private readonly bool _debug;
    
    public Bow(string fileName, bool debug=false)
    {
        _code = String.Join("\n", File.ReadAllLines(fileName));
        _debug = debug;
    }
    
    public void Run()
    {
        try
        {
            List<Token> tokens = new Lexer(_code).ScanTokens();
            if (_debug)
            {
                foreach (var token in tokens)
                {
                    Console.WriteLine(token.Inspect());
                }
            }

            List<Statement> statements = new Parser(tokens).Parse();
            new Interpreter(statements).Interpret();

            if (_debug)
            {
                Env.OutputVariables();
            }
        }
        catch (BowSyntaxError ex)
        {
            Console.WriteLine($"\x1B[91m{(_debug ? ex : ex.Message)}\x1B[0m");
        }
    }
}
