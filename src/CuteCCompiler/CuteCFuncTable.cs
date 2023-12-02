namespace CuteCCompiler;

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