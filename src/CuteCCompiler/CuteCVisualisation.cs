using System.Text;
using Spectre.Console;

namespace CuteCCompiler;

public static class CuteCVisualisation
{
    public static void DrawCompileSteps(
        string code,
        List<TokenWord> tokenWords,
        List<CuteToke> tokens,
        ProgramRoot programRoot,
        CuteCVariableTable variableTable,
        CuteCFuncTable funcTable,
        CuteCAsmToken[] asm,
        List<AsmInst> finalOutput
    )
    {
        var rightGrid = new Grid();

        var tbl = new Table().AddColumns("char", "Code", "Words");
        var zip = tokenWords.Zip(tokens);
        foreach (var (first, second) in zip)
        {
            tbl.AddRow(new Markup(second.Data.StartChar.ToString()), new Markup(first.Str),
                new Markup(second.Kind.ToString()));
        }

        var t = new Tree("");
        var varTblGfx = GetVarTableGraphic(variableTable);
        var stmtAssmTbl = GetStatementTable(asm);
        
        DrawNodeOneLineInfo(programRoot, t.AddNode("Program Syntax Translation"));
        t.AddNode("Var Table").AddNode(varTblGfx);
        t.AddNode("Function Table").AddNode(GetFuncTableGraphic(funcTable));
        t.AddNode("Statement List").AddNode(stmtAssmTbl);


        var leftGrid = new Grid();
        rightGrid.AddColumns(new GridColumn(), new GridColumn(), new GridColumn(), new GridColumn());
        leftGrid.AddColumns(new GridColumn(), new GridColumn());
        leftGrid.AddRow(rightGrid);
        rightGrid.AddRow(new Panel(code), new Panel(GetAsmText(finalOutput)), tbl, t);
        AnsiConsole.Write(rightGrid);
    }

    private static string GetAsmText(List<AsmInst> finalOutput)
    {
        var sb = new StringBuilder();
        foreach (var line in finalOutput)
        {
            sb.AppendLine(line.GetFileText());
        }

        return sb.ToString();
    }

    public static TreeNode GetStatementTable(CuteCAsmToken[] cuteCAsmTokens)
    {
        var ret = new TreeNode(new Markup("ASM"));
        var n1 = ret.AddNode("TokenInfo");
        foreach (var asm in cuteCAsmTokens)
        {
            var n2 = n1.AddNode(new Markup(asm.Node.GetOneLineInfo()));
            var n3 = n2.AddNode("STATEMENTS:");
            foreach (var inst in asm.Instructions)
            {
                n3.AddNode(inst.AssemblyToken.ToString());
            }
        }

        return ret;
    }

    public static Table GetFuncTableGraphic(CuteCFuncTable ft)
    {
        var ret = new Table();

        ret.AddColumns("NAMESPACE", "FN NAME");
        foreach (var kvp in ft.FuncDictionary)
        {
            foreach (var funcDef in kvp.Value)
            {
                var fnName = funcDef.Key;
                ret.AddRow(new Markup(kvp.Key), new Markup(fnName));
            }
        }

        return ret;
    }


    public static Table GetVarTableGraphic(CuteCVariableTable variableTable)
    {
        var ret = new Table();
        ret.AddColumns("Fullname", "Var Slot");
        foreach (var kvp in variableTable.VarTable)
        {
            ret.AddRow(new Markup(kvp.Key), new Markup(kvp.Value.ToString("000")));
        }

        return ret;
    }


    static void DrawNodeOneLineInfo(ICuteLexNode node, TreeNode parent)
    {
        var newParent = parent.AddNode(NameSpace.Combine(node.NameSpace, node.GetOneLineInfo()));
        foreach (var cn in node.Children)
        {
            DrawNodeOneLineInfo(cn, newParent);
        }
    }
}