namespace CuteCCompiler;

public class CuteCFuncTable
{
    // (namespace)  (function names in namespace) (function def) 
    internal readonly Dictionary<string, Dictionary<string, FuncDef>> FuncDictionary = new();

    public FuncDef FindFunctionInNameSpace(string ns, string funcName)
    {
        var inNs = FuncDictionary.ContainsKey(ns);
        var inGlobal = FuncDictionary["."].ContainsKey(funcName);
        if (!inNs && !inGlobal) throw new Exception($"Func not found {funcName} ");
        if (inNs) return FuncDictionary[ns][funcName];
        else return FuncDictionary["."][funcName];
    }

    public CuteCFuncTable(ProgramRoot rootToken)
    {
        var fns = rootToken.FindFuncDefs();
        foreach (var fn in fns)
        {
            if (!FuncDictionary.ContainsKey(fn.NameSpace))
                FuncDictionary.Add(fn.NameSpace, new Dictionary<string, FuncDef>());

            if (!FuncDictionary[fn.NameSpace].ContainsKey(fn.FuncName.Data.Str))
                FuncDictionary[fn.NameSpace].Add(fn.FuncName.Data.Str, fn);

            FuncDictionary[fn.NameSpace][fn.FuncName.Data.Str] = fn;
        }
    }
}