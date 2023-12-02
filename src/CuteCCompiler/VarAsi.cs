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
    public List<CuteToke> ChildData { get; } = new();
    public List<CuteToke> ExpressionData { get; }
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarAssignment;
    public string? ProvidedNameSpace => null;
    public string NameSpace { get; set; }
    public string GetOneLineInfo() => $"VAR ASSIGNMENT | {VarBeingAssignedTo.Data.Str} = {((ICuteLexNode)this).Expression}";

    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt, CuteCFuncTable ft)
    {
        throw new NotImplementedException();
    }


    public List<ICuteLexNode> Children { get; set; } = new();
}