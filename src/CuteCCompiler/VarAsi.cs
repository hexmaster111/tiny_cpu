using TinyAssemblerLib;

namespace CuteCCompiler;

public class VarAsi : ICuteLexNode
{
    public VarAsi(TokenStream ts, string ns)
    {
        NameSpace = ns;
        var exprTokens = ts.ReadEndLineType();
        var exprStream = new TokenStream(exprTokens.ToArray());
        VarBeingAssignedTo = exprStream.TakeOne();
        var keyWord = exprStream.TakeOne();
        if (keyWord.Kind != CuteTokenKind.Assignment) throw new Exception("Incorrect keyword!");
        ExpressionData = exprStream.ReadEndLineType();
    }

    public CuteToke VarBeingAssignedTo { get; }
    public string VariableFullName => CuteCCompiler.NameSpace.Combine(NameSpace, VarBeingAssignedTo.Data.Str);
    
    public List<CuteToke> ChildData { get; } = new();
    public List<CuteToke> ExpressionData { get; }
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarAssignment;
    public string? ProvidedNameSpace => null;
    public string NameSpace { get; set; }

    public string GetOneLineInfo() =>
        $"VAR ASSIGNMENT | {VarBeingAssignedTo.Data.Str} = {((ICuteLexNode)this).Expression}";

    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt, CuteCFuncTable ft)
    {
        var exp = ((ICuteLexNode)this).Expression;
        if (exp.GetType() == typeof(ConstantExpression))
        {
            var e = exp as ConstantExpression;
            if (string.IsNullOrWhiteSpace(e.AsmStringValue)) throw new Exception("Missing arg");

            if (string.IsNullOrWhiteSpace(
                    Expression.CreateAsmStringValue(vt.GetVariableNumber(VarBeingAssignedTo, NameSpace))
                )) throw new Exception("Missing arg");

            var list = new List<AsmInst>();
            list.Add(new(new TinyAsmTokenizer.Token(
                TinyAsmTokenizer.Token.TokenType.SETREG,
                TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                TinyAsmTokenizer.Token.ArgumentType.CONST,
                TinyCCallConventions.ScratchRegister0.ToString(),
                e.AsmStringValue)
            ));
            list.Add(new(new TinyAsmTokenizer.Token(
                TinyAsmTokenizer.Token.TokenType.MEM_WRITE,
                TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                TinyAsmTokenizer.Token.ArgumentType.CONST,
                TinyCCallConventions.ScratchRegister0.ToString(),
                Expression.CreateAsmStringValue(vt.GetVariableNumber(VarBeingAssignedTo, NameSpace))
            )));
            return list;
        }

        if (exp.GetType() == typeof(VarDef))
        {
            throw new NotImplementedException();
        }

        if (exp.GetType() == typeof(VariableExpression))
        {
            var e = exp as VariableExpression;

            var destVarSlot = vt.GetVariableNumber(VarBeingAssignedTo, NameSpace);
            var srcVarSlot = vt.GetVariableNumber(e.Variable, NameSpace);

            var ret = new List<AsmInst>
            {
                new(new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.MEM_READ,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.CONST,
                    TinyCCallConventions.ScratchRegister0.ToString(),
                    Expression.CreateAsmStringValue(srcVarSlot)
                )),
                new(new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.MEM_WRITE,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.CONST,
                    TinyCCallConventions.ScratchRegister0.ToString(),
                    Expression.CreateAsmStringValue(destVarSlot)
                ))
            };

            return ret;
        }


        if (exp.GetType() == typeof(NothingExpression))
        {
            var destVarSlot = vt.GetVariableNumber(VarBeingAssignedTo, NameSpace);
            var ret = new List<AsmInst>
            {
                new(new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.SETREG,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.CONST,
                    TinyCCallConventions.ScratchRegister0.ToString(),
                    Expression.CreateAsmStringValue("0")
                )),
                new(new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.MEM_WRITE,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.CONST,
                    TinyCCallConventions.ScratchRegister0.ToString(),
                    Expression.CreateAsmStringValue(destVarSlot)
                ))
            };

            return ret;
        }

        throw new NotImplementedException();
    }


    public List<ICuteLexNode> Children { get; set; } = new();
}