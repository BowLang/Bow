using Parse.Environment;

namespace Parse.Statements;

public class Statement
{
    public virtual void Interpret()
    {
        throw new NotImplementedException();
    }

    public virtual string Interpret(bool lastInShell)
    {
        Interpret();

        return "";
    }
}
