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
    private readonly bool _inShell;
    
    public Bow(string fileName, bool inShell=false, bool debug=false)
    {
        _code = inShell ? fileName : String.Join("\n", File.ReadAllLines(fileName));
        _debug = debug;
        _inShell = inShell;
    }
    
    public void Run()
    {
        string nextCode = "";

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
            new Interpreter(statements).Interpret(_inShell);
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
        catch (BowEOFError ex)
        {
            if (_inShell)
            {
                nextCode = _code;
            }
            else
            {
                Console.WriteLine($"\x1B[91mBow EOF Error: {(_debug ? ex : ex.Message)}\x1B[0m");
            }
        }        
        finally
        {
            if (_debug)
            {
                Env.Output();
            }

            if (_inShell)
            {
                RunShell(false, nextCode);
            }
        }
    }

    public static void RunShell(bool firstCall=true, string lines="")
    {
        if (firstCall) Console.WriteLine("\x1B[1;4mBow Interactive Shell\x1B[0m\n");

        string prompt = lines == "" ? "bow" : "   ";
        Console.Write($"\x1B[92m{prompt}>\x1B[0m ");
        
        string? line = Console.ReadLine();
        line ??= "";
        
        if (line == "exit")
        {
            return;
        }
        
        lines += line + "\n";
        
        new Bow(lines, true).Run();
    }
}
