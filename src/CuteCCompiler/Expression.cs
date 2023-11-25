using System.Diagnostics;
using System.Linq.Expressions;

namespace CuteCCompiler;

public class VariableExpression : Expression
{
    public CuteToke Variable { get; }
    public override string ToString() => "VarExp (" + Variable.Data.Str + ")";

    public VariableExpression(List<CuteToke> exp) : base(exp)
    {
        Variable = exp[0];
    }
}

public class ConstantExpression : Expression
{
    public CuteToke Value { get; }
    public string AsmStringValue => ParseAsmStringValue(Value.Data.Str);
    
    public override string ToString() => "Const Expr (" + Value.Data.Str + ")";

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

    public override string ToString() => $"Math Expr ({Left}) {Opp.Data.Str} ({Right})";

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

public class FuncCallExpression : Expression
{
    public CuteToke Arg
    {
        get
        {
            if (ExpData.Count > 2) return ExpData[2];
            return CuteToke.Void;
        }
    }

    public CuteToke FuncName
    {
        get
        {
            if (ExpData.Count > 0) return ExpData[0];
            return CuteToke.Void;
        }
    }


    public FuncCallExpression(List<CuteToke> exp) : base(exp)
    {
    }

    public override string ToString() => $"({FuncName.Data.Str} ( {Arg.Data.Str} ))";
}

public class Expression
{
    
     
    public static string ParseAsmStringValue(string tokenValue)
    {
        return "0x" + int.Parse(tokenValue).ToString("X2");
    }
    
    protected static List<List<CuteToke>> SplitFunctionArgVarNames(List<CuteToke> parenBody)
    {
        var ts = new TokenStream(parenBody.ToArray());
        var buff = new List<CuteToke>();
        var ret = new List<List<CuteToke>>();
        while (ts.Next(out var token))
        {
            Debug.Assert(token != null, nameof(token) + " != null");
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

        if (ts.Peek(0)!.Kind == CuteTokenKind.Assignment) ts.Next();

        List<CuteToke> buff = new();
        while (ts.Next(out var c))
        {
            Debug.Assert(c != null, nameof(c) + " != null");
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

            if (buff[0].Kind == CuteTokenKind.VarName && (buff.Count > 1 && buff[1].Kind == CuteTokenKind.EndLine))
            {
                buff.AddRange(ts.ReadEndLineType());
                return new VariableExpression(buff);
            }

            if (buff[0].Kind == CuteTokenKind.VarName && exp.Count == 1)
            {
                return new VariableExpression(buff);
            }


            if (buff.Count > 2)
                if (buff[0].Kind == CuteTokenKind.VarName && CuteTokenKindExt.ValueTypes.Contains(buff[2].Kind))
                {
                    buff.AddRange(ts.ReadEndLineType());
                    return new FuncCallExpression(buff);
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