using Errors;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.ObjInstances;

namespace Parse.Statements;

public class Declaration : Statement
{
    private readonly string _name;
    private readonly string _type;
    private readonly Expression _valueExpression;
    private readonly int _line;
    private readonly bool _isConstant;
    
    public Declaration(string name, Expression valueExpression, string type, bool isConstant, int line)
    {
        _name  = name;
        _valueExpression = valueExpression;
        _type  = type;
        _line  = line;
        _isConstant = isConstant;
    }

    public override void Interpret()
    {
        ObjInstance newValue = _valueExpression.Evaluate();

        ObjectSymbol type = Env.GetObject(_type, _line);

        if (Env.IsVariableDefinedLocally(_name))
        {
            throw new BowSyntaxError($"Can't re-declare variable '{_name}' on line {_line}");
        }

        if (!newValue.IsAcceptedBy(type))
        {
            throw new BowTypeError(
                $"Can't assign {newValue.DisplayName()} to variable of type {type.DisplayName()} on line {_line}");
        }
        
        VariableSymbol symbol = new VariableSymbol(_name, newValue, _line, _isConstant);

        Env.AddVariable(symbol);
    }

    public override string Interpret(bool lastInShell)
    {
        Interpret();
        
        return lastInShell ? Env.GetVariable(_name).Object.DisplayValue() : "";
    }
}
