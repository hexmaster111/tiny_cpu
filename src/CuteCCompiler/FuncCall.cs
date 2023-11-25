namespace CuteCCompiler;

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

    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt)
    {
        throw new NotImplementedException();
    }
}