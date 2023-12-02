using TinyAssemblerLib;

namespace CuteCCompiler;

public record AsmInst(TinyAsmTokenizer.Token AssemblyToken)
{
    public string GetFileText() =>
     $"{AssemblyToken.Type} {AssemblyToken.ArgumentZeroData} {AssemblyToken.ArgumentOneData}";
}
