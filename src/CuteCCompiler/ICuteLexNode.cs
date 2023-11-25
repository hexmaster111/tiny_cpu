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
    List<AsmInst> ExpelInstructions(CuteCVariableTable vt);
}