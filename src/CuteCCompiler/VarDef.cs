using TinyAssemblerLib;

namespace CuteCCompiler;

public class VarDef : ICuteLexNode
{
    public VarDef(TokenStream ts, string nameSpace)
    {
        var expr = ts.ReadEndLineType().ToArray();
        VariableType = expr[0];
        VariableName = expr[1];
        ExpressionData = expr[2..].ToList();
        NameSpace = nameSpace;
    }

    public CuteToke VariableType { get; }
    public CuteToke VariableName { get; }
    public string? ProvidedNameSpace => null; //Nothing can be in a var def namespace

    public string GetOneLineInfo() =>
        $"VAR DEF | {VariableName.Data.Str} : {VariableType.Data.Str} = {((ICuteLexNode)this).Expression}";


    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt, CuteCFuncTable ft)
    {
        var exp = ((ICuteLexNode)this).Expression;
        if (exp.GetType() == typeof(ConstantExpression))
        {
            var e = exp as ConstantExpression;
            if (string.IsNullOrWhiteSpace(e.AsmStringValue)) throw new Exception("Missing arg");

            if (string.IsNullOrWhiteSpace(
                    Expression.CreateAsmStringValue(vt.VarTable[VariableFullName].ToString())
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
                Expression.CreateAsmStringValue(vt.VarTable[VariableFullName].ToString())
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

            var destVarSlot = vt.GetVariableNumber(VariableName, NameSpace);
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
            var destVarSlot = vt.GetVariableNumber(VariableName, NameSpace);
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

        // ret.Add(new AsmInst( new TinyAsmTokenizer.Token()));


        throw new NotImplementedException();
    }

    public string NameSpace { get; set; }
    public string VariableFullName => CuteCCompiler.NameSpace.Combine(NameSpace, VariableName.Data.Str);

    public List<CuteToke> ExpressionData { get; }
    public List<CuteToke> ChildData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarDefinition;
    public List<ICuteLexNode> Children { get; set; } = new();
}