using Tokenise;

namespace Parse.Statements;

public class Statement
{
    public virtual void Interpret()
    {
        throw new NotImplementedException();
    }

    public virtual string Interpret(bool lastInShell)
    {
        throw new NotImplementedException();
    }
}
