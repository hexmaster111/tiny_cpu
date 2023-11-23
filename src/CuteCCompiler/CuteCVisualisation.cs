using Spectre.Console;

namespace CuteCCompiler;

public static class CuteCVisualisation
{
    public static void DrawCompileSteps(List<TokenWord> tokenWords, List<CuteToke> tokens)
    {
        var tbl = new Table().AddColumns("Words", "Tokens");
        var zip = tokenWords.Zip(tokens);
        foreach (var (first, second) in zip)
        {
            tbl.AddRow(new Markup(first.Str), new Markup(second.Kind.ToString()));
        }

        AnsiConsole.Write(tbl);
    }
}