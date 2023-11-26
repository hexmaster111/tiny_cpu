using Spectre.Console;
using Spectre.Console.Json;

namespace CuteCCompiler;

public static class CuteCVisualisation
{
    public static void DrawCompileSteps(string code, List<TokenWord> tokenWords, List<CuteToke> tokens,
        ProgramRoot programRoot, CuteCVariableTable variableTable, CuteCAsmToken[] asm, List<AsmInst> finalOutput)
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
        DrawNodeOneLineInfo(programRoot, t.AddNode("Program Syntax Translation"));

        var varTblGfx = GetVarTableGraphic(variableTable);

        t.AddNode("Var Table").AddNode(varTblGfx);

        var stmtAssmTbl = GetStatementTable(asm);

        t.AddNode("Statement List").AddNode(stmtAssmTbl);

        var leftGrid = new Grid();
        rightGrid.AddColumns(new GridColumn(), new GridColumn(), new GridColumn());
        leftGrid.AddColumns(new GridColumn(), new GridColumn());
        leftGrid.AddRow(rightGrid);
        rightGrid.AddRow(new Panel(code), tbl, t);
        // grid.AddRow(new JsonText(Newtonsoft.Json.JsonConvert.SerializeObject(programRoot)));
        AnsiConsole.Write(rightGrid);
    }

    private static TreeNode GetStatementTable(CuteCAsmToken[] cuteCAsmTokens)
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


    private static Table GetVarTableGraphic(CuteCVariableTable variableTable)
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
        var newParent = parent.AddNode(node.NameSpace + "   " + node.GetOneLineInfo());
        foreach (var cn in node.Children)
        {
            DrawNodeOneLineInfo(cn, newParent);
        }
    }
}