using System.Linq.Expressions;

namespace CuteCCompiler;

public class VariableExpression : Expression
{
    public CuteToke Variable { get; }
    public override string ToString() => Variable.Data.Str;

    public VariableExpression(List<CuteToke> exp) : base(exp)
    {
        Variable = exp[0];
    }
}

public class ConstantExpression : Expression
{
    public CuteToke Value { get; }

    public override string ToString() => Value.Data.Str;

    public ConstantExpression(List<CuteToke> exp) : base(exp)
    {
        Value = exp[0];
    }
}

public class MathExpression : Expression
{
    public Expression Left { get; }
    public Expression Right { get; }
    public CuteToke Opp { get; }

    public override string ToString() => $"{Left} {Opp.Data.Str} {Right}";

    public MathExpression(List<CuteToke> exp) : base(exp)
    {
        
        Left = FromData(exp[0]);
        Opp = exp[1];
        Right = FromData(exp[2]);
    }

    private Expression FromData(CuteToke exp) => FromData(new List<CuteToke>() { exp });
}

public class NothingExpression : Expression
{
    public NothingExpression(List<CuteToke> exp) : base(exp)
    {
    }

    public override string ToString() => "(nothing)";
}

public class Expression
{
    protected static List<List<CuteToke>> SplitFunctionArgVarNames(List<CuteToke> parenBody)
    {
        var ts = new TokenStream(parenBody.ToArray());
        var buff = new List<CuteToke>();
        var ret = new List<List<CuteToke>>();
        while (ts.Next(out var token))
        {
            if (token.Kind == CuteTokenKind.Comma)
            {
                ret.Add(buff);
                buff = new();
                continue;
            }

            buff.Add(token);
        }

        ret.Add(buff);

        return ret;
    }

    public static Expression FromData(List<CuteToke> exp)
    {
        var ts = new TokenStream(exp.ToArray());

        if (ts.Peek(0).Kind == CuteTokenKind.Assignment) ts.Next();

        List<CuteToke> buff = new();
        while (ts.Next(out CuteToke? c))
        {
            buff.Add(c);

            if (buff[0].Kind == CuteTokenKind.TypedValue)
            {
                buff.AddRange(ts.ReadEndLineType());
                return new ConstantExpression(buff);
            }

            if (buff[0].Kind == CuteTokenKind.EndLine)
            {
                buff.AddRange(ts.ReadEndLineType());
                return new NothingExpression(buff);
            }


            if (buff[0].Kind == CuteTokenKind.VarName && exp.Count == 1)
            {
                return new VariableExpression(buff);
            }


            if (buff.Count > 1)
                if (CuteTokenKindExt.MathTokens.Contains(buff[1].Kind))
                {
                    buff.AddRange(ts.ReadEndLineType());
                    return new MathExpression(buff);
                }
        }

        throw new Exception("unknown token pattern!");
    }


    public List<CuteToke> ExpData { get; }

    public Expression(List<CuteToke> exp)
    {
        ExpData = exp;
    }
}