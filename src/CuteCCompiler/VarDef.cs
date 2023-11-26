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
        $"{VariableName.Data.Str} : {VariableType.Data.Str} = {((ICuteLexNode)this).Expression}";

    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt)
    {
        var exp = ((ICuteLexNode)this).Expression;
        if (exp.GetType() == typeof(ConstantExpression))
        {
            var e = exp as ConstantExpression;
            return new List<AsmInst>()
            {
                new(new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.SETREG,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.CONST,
                    TinyCCallConventions.ScratchRegister0.ToString(),
                    e.AsmStringValue)
                ),
                new(new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.MEM_WRITE,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.CONST,
                    TinyCCallConventions.ScratchRegister0.ToString(),
                    Expression.ParseAsmStringValue(vt.VarTable[VariableFullName].ToString())
                ))
            };
        }


      
        if (exp.GetType() == typeof(VarDef))
        {
            throw new NotImplementedException();
        }

        if (exp.GetType() == typeof(VariableExpression))
        {
            var e = exp as VariableExpression;
            var ret = new List<AsmInst>();
            ret.Add(new AsmInst(
                new TinyAsmTokenizer.Token(
                    TinyAsmTokenizer.Token.TokenType.SETREG,
                    TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                    TinyAsmTokenizer.Token.ArgumentType.NONE,
                    TinyCCallConventions.ScratchRegister0.ToString())
                ));
            ret.Add(new (new TinyAsmTokenizer.Token(
                 TinyAsmTokenizer.Token.TokenType.MEM_WRITE,
                 TinyAsmTokenizer.Token.ArgumentType.REGISTER,
                 TinyAsmTokenizer.Token.ArgumentType.CONST,
                 vt.GetVariableNumber(VariableName, NameSpace)
                )));
            
            return ret;
        }


        // ret.Add(new AsmInst( new TinyAsmTokenizer.Token()));


        throw new NotImplementedException();
    }

    public string NameSpace { get; set; }
    public string VariableFullName => NameSpace + VariableName.Data.Str;

    public List<CuteToke> ExpressionData { get; }
    public List<CuteToke> ChildData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarDefinition;
    public List<ICuteLexNode> Children { get; set; } = new();
}