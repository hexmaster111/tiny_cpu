using System.Diagnostics;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int globalVarA = 42;
            fn main():void {
                int lv_demo=globalVarA;
            }
            fn other_func ( ) : void { int x=0; }
            main ();
            """;


        var words = CuteCWordSplitter.Wordify(input);
        var tokens = CuteCTokenizer.Tokenize(words);
        var rootToken = new ProgramRoot(tokens);
        CuteCLexer.Lex(rootToken);
        var varTable = CuteCVariableTable.MakeTable(rootToken);
        var funcTable = new CuteCFuncTable(rootToken);
        var asm = CuteCAsmToken.FromTree(varTable, funcTable, rootToken);
        var finalOutput = CuteCAsmToken.ConvertToAsm(asm);
        CuteCVisualisation.DrawCompileSteps(input, words, tokens, rootToken, varTable, asm, finalOutput);
        Debugger.Break();
    }
}

public class CuteCFuncTable
{
    // (namespace)  (function names in namespace) (function def) 
    private Dictionary<string, Dictionary<string, FuncDef>> _funcDictionary = new();
    public FuncDef FindFunctionInNameSpace(string ns, string funcName)
    {
        return _funcDictionary[ns][funcName];
    }

    public CuteCFuncTable(ProgramRoot rootToken)
    {
        var fns = rootToken.FindFuncDefs();
        foreach (var fn in fns)
        {
            if (!_funcDictionary.ContainsKey(fn.NameSpace))
                _funcDictionary.Add(fn.NameSpace, new Dictionary<string, FuncDef>());

            if (!_funcDictionary[fn.NameSpace].ContainsKey(fn.FuncName.Data.Str))
                _funcDictionary[fn.NameSpace].Add(fn.FuncName.Data.Str, fn);

            _funcDictionary[fn.NameSpace][fn.FuncName.Data.Str] = fn;
        }
    }
}