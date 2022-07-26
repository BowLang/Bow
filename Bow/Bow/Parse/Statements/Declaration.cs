using Errors;
using Parse.Environment;
using Parse.Expressions;
using Parse.Expressions.Literals;

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
        Literal value = _valueExpression.Evaluate();

        if (Env.IsVariableDefined(_name))
        {
            throw new BowSyntaxError($"Can't re-declare variable '{_name}' on line {_line}");
        }

        if (_type != value.Type)
        {
            throw new BowSyntaxError($"Can't assign {value.Type} to variable of type {_type[..3]} on line {_line}");
        }
        
        Symbol symbol = new Symbol(_name, _type, value.Value, _line, _isConstant);

        Env.AddVariable(symbol);
    }
}
