using System.Diagnostics;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            //var table
            // global::a - 0x00
            int a = 42;


            //global::fn_main::c - 0x01
            fn main (  ) : void
            {
                int c = a + 69;
            }

            fn other_func():void{int c=0;}

            main();
            """;


        var words = CuteCWordSplitter.Wordify(input);
        var tokens = CuteCTokenizer.Tokenize(words);
        var rootToken = new ProgramRoot(tokens);
        CuteCLexer.Lex(rootToken);

        var varTable = CuteCVariableTable.MakeTable(rootToken);

        CuteCVisualisation.DrawCompileSteps(input, words, tokens, rootToken, varTable);
        Debugger.Break();
    }
}

public class CuteCVariableTable
{
    public Dictionary<string, int> VarTable { get; } = new();

    private int _varIdCounter = 0;

    public void AddVariable(string name)
    {
        if (VarTable.ContainsKey(name)) throw new Exception($"Var {name} was all ready defined");
        VarTable.Add(name, _varIdCounter++);
    }

    public static CuteCVariableTable MakeTable(ProgramRoot rootToken)
    {
        var ret = new CuteCVariableTable();

        var vars = rootToken.FindVarDefs();
        foreach (var varDef in vars)
        {
            var s = varDef.VariableFullName;
            ret.AddVariable(s);
        }

        return ret;
    }

    private CuteCVariableTable()
    {
    }
}