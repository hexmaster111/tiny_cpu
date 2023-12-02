using TinyAssemblerLib;

namespace CuteCCompiler;

public class FuncDef : ICuteLexNode
{
    public FuncDef(TokenStream ts, string ns)
    {
        NameSpace = ns;
        var fnKeyWord = ts.TakeOne();
        FuncName = ts.TakeOne();
        ArgData = ts.ReadParenBody();
        var ofTypeKeyWord = ts.TakeOne();
        Return = ts.TakeOne();
        ChildData = ts.ReadBracketBody();
    }

    public CuteToke FuncName { get; }
    public List<CuteToke> ArgData { get; }

    public CuteToke ArgType
    {
        get
        {
            if (ArgData.Count > 0) return ArgData[0];
            return new CuteToke(CuteTokenKind.Type, new TokenWord("void", -1));
        }
    }

    public CuteToke LocalVarName
    {
        get
        {
            if (ArgData.Count > 1) return ArgData[1];
            return new CuteToke(CuteTokenKind.ERROR, new TokenWord(string.Empty, -1));
        }
    }

    public CuteToke Return { get; }

    public List<CuteToke> ChildData { get; }
    public List<CuteToke> ExpressionData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.FuncDefinition;
    public List<ICuteLexNode> Children { get; set; } = new();
    public string ProvidedNameSpace => $"fn_{FuncName.Data.Str}";
    public string NameSpace { get; set; }
    public string CallableName => CuteCCompiler.NameSpace.Combine(NameSpace, FuncName.Data.Str);

    public string GetOneLineInfo() =>
        $"fn {FuncName.Data.Str} ({ArgType.Data.Str} {LocalVarName.Data.Str}) => {Return.Data.Str}";


    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt, CuteCFuncTable ft)
    {
        var ret = new List<AsmInst>
        {
            new AsmInst(new TinyAsmTokenizer.Token(
                TinyAsmTokenizer.Token.TokenType.LBL,
                TinyAsmTokenizer.Token.ArgumentType.STR,
                TinyAsmTokenizer.Token.ArgumentType.NONE,
                CuteCCompiler.NameSpace.Combine(NameSpace, FuncName.Data.Str)
            ))
        };


        foreach (var childNode in Children)
        {
            ret.AddRange(childNode.ExpelInstructions(vt, ft));
        }

        ret.Add(new AsmInst(new TinyAsmTokenizer.Token(TinyAsmTokenizer.Token.TokenType.RET)));

        return ret;
    }
}