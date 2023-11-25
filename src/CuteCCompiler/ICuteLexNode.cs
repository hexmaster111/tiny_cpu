namespace CuteCCompiler;

public interface ICuteLexNode
{
    /// Data to be used by next call to LEX
    public List<CuteToke> ChildData { get; }

    public List<CuteToke> ExpressionData { get; }

    public CuteLexNodeKind Kind { get; }

    public List<ICuteLexNode> Children { get; set; }
    public Expression Expression => Expression.FromData(ExpressionData);
    public string ProvidedNameSpace { get; }
    public string? NameSpace { get; set; }
    string GetOneLineInfo();
}

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

    public string NameSpace { get; set; }
    public string VariableFullName => NameSpace + VariableName.Data.Str;

    public List<CuteToke> ExpressionData { get; }
    public List<CuteToke> ChildData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarDefinition;
    public List<ICuteLexNode> Children { get; set; } = new();
}

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

    public string GetOneLineInfo() =>
        $"fn {FuncName.Data.Str} ({ArgType.Data.Str} {LocalVarName.Data.Str}) => {Return.Data.Str}";
}

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
    public List<CuteToke> ChildData { get; } = new();
    public List<CuteToke> ExpressionData { get; }
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarAssignment;
    public string? ProvidedNameSpace => null;
    public string NameSpace { get; set; }
    public string GetOneLineInfo() => $"{VarBeingAssignedTo.Data.Str} = {((ICuteLexNode)this).Expression}";

    public List<ICuteLexNode> Children { get; set; } = new();
}

public class FuncCall : ICuteLexNode
{
    public FuncCall(TokenStream ts, string ns)
    {
        NameSpace = ns;
        FunctionNameBeingCalled = ts.TakeOne();
        ExpressionData = ts.ReadParenBody();
        var endLine = ts.TakeOne();
    }

    public CuteToke Argument
    {
        get
        {
            if (ExpressionData.Count > 0) return ExpressionData[0];
            return CuteToke.Void;
        }
    }

    public List<CuteToke> ExpressionData { get; }
    public CuteToke FunctionNameBeingCalled { get; }
    public CuteLexNodeKind Kind => CuteLexNodeKind.FuncCall;
    public List<ICuteLexNode> Children { get; set; } = new();
    public List<CuteToke> ChildData { get; } = new();
    public string? ProvidedNameSpace => null;
    public string NameSpace { get; set; }
    public string GetOneLineInfo() => $"{FunctionNameBeingCalled.Data.Str} ({Argument.Data.Str})";
}

public class ProgramRoot : ICuteLexNode
{
    public ProgramRoot(List<CuteToke> tokens) => ChildData = tokens;

    public List<CuteToke> ChildData { get; }
    public List<CuteToke> ExpressionData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.ProgramRoot;
    public List<ICuteLexNode> Children { get; set; } = new();
    public string ProvidedNameSpace => throw new InvalidOperationException();
    public string NameSpace { get; set; }
    public string GetOneLineInfo() => " Everything in the program";

    public List<VarDef> FindVarDefs()
    {
        var ret = new List<ICuteLexNode>();
        Find(this, ret, CuteLexNodeKind.VarDefinition);
        return ret.Cast<VarDef>().ToList();
    }

    private void Find(ICuteLexNode current, List<ICuteLexNode> list, CuteLexNodeKind kindToFind)
    {
        if (current.Kind == kindToFind)
        {
            list.Add(current);
        }

        foreach (var child in current.Children)
        {
            Find(child, list, kindToFind);
        }
    }
}