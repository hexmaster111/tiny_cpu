namespace CuteCCompiler;

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
        var fullName = NameSpace.Combine(ns, variableName.Data.Str);

        if (!VarTable.TryGetValue(fullName, out int id)) throw new Exception("Variable Not found");

        return id.ToString();
    }
}