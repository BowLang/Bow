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
    
    public Bow(string fileName, bool shell=false, bool debug=false)
    {
        _code = shell ? fileName : String.Join("\n", File.ReadAllLines(fileName));
        _debug = debug;
    }
    
    public void Run(bool inShell=false)
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
            new Interpreter(statements).Interpret(inShell);
        }
        catch (BowSyntaxError ex)
        {
            Console.WriteLine($"\x1B[91mBow Syntax Error: {(_debug ? ex : ex.Message)}\x1B[0m");
        }
        catch (BowTypeError ex)
        {
            Console.WriteLine($"\x1B[91mBow Type Error: {(_debug ? ex : ex.Message)}\x1B[0m");
        }
        catch (BowNameError ex)
        {
            Console.WriteLine($"\x1B[91mBow Name Error: {(_debug ? ex : ex.Message)}\x1B[0m");
        }
        finally
        {
            if (_debug)
            {
                Env.OutputVariables();
            }
        }
    }

    public static void RunShell()
    {
        Console.WriteLine("\x1B[1;4mBow Interactive Shell\x1B[0m\n");
        
        while (true)
        {
            Console.Write("\x1B[92mbow>\x1B[0m ");
            string? line = Console.ReadLine();
            line ??= "";
            
            if (line == "exit")
            {
                break;
            }
            
            new Bow(line, true).Run(true);
        }
    }
}
