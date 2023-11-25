using System.Diagnostics;
using TinyAssemblerLib;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            //var table
            // global::a - 0x00
            int globalVarA = 42;


            //global::fn_main::c - 0x01
            fn main (  ) : void
            {
                int lv_demo = globalVarA;
            }

            fn other_func():void{int c=0;}

            main();
            """;


        var words = CuteCWordSplitter.Wordify(input);
        var tokens = CuteCTokenizer.Tokenize(words);
        var rootToken = new ProgramRoot(tokens);
        CuteCLexer.Lex(rootToken);
        var varTable = CuteCVariableTable.MakeTable(rootToken);
        var asm = CuteCAsmToken.FromTree(varTable, rootToken);
        CuteCVisualisation.DrawCompileSteps(input, words, tokens, rootToken, varTable, asm);
        Debugger.Break();
    }
}

public class CuteCFunctionTable
{
}

public class AsmInst
{
    public TinyAsmTokenizer.Token AssemblyToken { get; }
    public AsmInst(TinyAsmTokenizer.Token asmToken)
    {
        //TODO: Add location info for debugger

        AssemblyToken = asmToken;
    }
}

public class CuteCAsmToken
{
    public ICuteLexNode Node { get; }
    public List<AsmInst> Instructions { get; }

    public CuteCAsmToken(ICuteLexNode node, CuteCVariableTable vt)
    {
        Node = node;
        Instructions = Node.ExpelInstructions(vt);
    }

    public static CuteCAsmToken[] FromTree(CuteCVariableTable variableTable, ProgramRoot programRoot)
    {
        var ret = new List<CuteCAsmToken>();
        UnwrapTree(programRoot, variableTable, ret);
        return ret.ToArray();
    }

    private static void UnwrapTree(ICuteLexNode c, CuteCVariableTable variableTable, List<CuteCAsmToken> currPrg)
    {
        currPrg.Add(new CuteCAsmToken(c, variableTable));
        foreach (var child in c.Children)
        {
            UnwrapTree(child, variableTable, currPrg);
        }
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

    public string GetVariableNumber(CuteToke variableName, string ns)
    {
        var fullName = ns + variableName.Data.Str;

        if (!VarTable.TryGetValue(fullName, out int id)) throw new Exception("Variable Not found");

        return id.ToString();
    }
}