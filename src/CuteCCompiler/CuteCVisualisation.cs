using Spectre.Console;
using Spectre.Console.Json;

namespace CuteCCompiler;

public static class CuteCVisualisation
{
    public static void DrawCompileSteps(string code, List<TokenWord> tokenWords, List<CuteToke> tokens,
        ProgramRoot programRoot)
    {
        var grid = new Grid();

        var tbl = new Table().AddColumns("Code", "Words");
        var zip = tokenWords.Zip(tokens);
        foreach (var (first, second) in zip)
        {
            tbl.AddRow(new Markup(first.Str), new Markup(second.Kind.ToString()));
        }

        var t = new Tree("");
        Print(programRoot, t.AddNode(""));

        grid.AddColumns(
            new GridColumn()
            {
            }, new GridColumn()
            {
            }
            , new GridColumn()
            {
            }
        );
        grid.AddRow(new Panel(code), tbl, t);
        // grid.AddRow(new JsonText(Newtonsoft.Json.JsonConvert.SerializeObject(programRoot)));
        AnsiConsole.Write(grid);
    }

    static void Print(ICuteLexNode node, TreeNode parent)
    {
        var newParent = parent.AddNode(node.GetOneLineInfo());
        foreach (var cn in node.Children)
        {
            Print(cn, newParent);
        }
    }
}