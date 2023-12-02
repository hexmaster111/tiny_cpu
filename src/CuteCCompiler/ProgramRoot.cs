namespace CuteCCompiler;

public class ProgramRoot : ICuteLexNode
{
    public ProgramRoot(List<CuteToke> tokens) => ChildData = tokens;

    public List<CuteToke> ChildData { get; }
    public List<CuteToke> ExpressionData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.ProgramRoot;
    public List<ICuteLexNode> Children { get; set; } = new();
    public string ProvidedNameSpace => RootNameSpace;
    public string NameSpace { get; set; }
    public const string RootNameSpace = ".";

    public string GetOneLineInfo() => " Everything in the program";

    public List<AsmInst> ExpelInstructions(CuteCVariableTable vt, CuteCFuncTable ft)
        => new();

    public List<VarDef> FindVarDefs()
    {
        var ret = new List<ICuteLexNode>();
        Find(this, ret, CuteLexNodeKind.VarDefinition);
        return ret.Cast<VarDef>().ToList();
    }

    public List<FuncDef> FindFuncDefs()
    {
        var ret = new List<ICuteLexNode>();
        Find(this, ret, CuteLexNodeKind.FuncDefinition);
        return ret.Cast<FuncDef>().ToList();
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