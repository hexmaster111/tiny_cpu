namespace CuteCCompiler;

public class CuteCAsmToken
{
    public ICuteLexNode Node { get; }
    public List<AsmInst> Instructions { get; }

    public CuteCAsmToken(ICuteLexNode node, CuteCVariableTable vt, CuteCFuncTable ft)
    {
        Node = node;
        Instructions = Node.ExpelInstructions(vt, ft);
    }

    public static CuteCAsmToken[] FromTree(
        CuteCVariableTable variableTable,
        CuteCFuncTable ft,
        ProgramRoot programRoot
    )
    {
        var ret = new List<CuteCAsmToken>();
        UnwrapTree(programRoot, variableTable, ft, ret);
        return ret.ToArray();
    }

    private static void UnwrapTree(
        ICuteLexNode c,
        CuteCVariableTable variableTable,
        CuteCFuncTable ft,
        List<CuteCAsmToken> currPrg
    )
    {
        currPrg.Add(new CuteCAsmToken(c, variableTable, ft));
        foreach (var child in c.Children)
        {
            UnwrapTree(child, variableTable, ft, currPrg);
        }
    }

    public static List<AsmInst> ConvertToAsm(CuteCAsmToken[] asm)
    {
        var all = asm.Select(x => x.Instructions);
        var ret = new List<AsmInst>();
        foreach (var some in all)
        {
            ret.AddRange(some);
        }

        return ret;
    }
}