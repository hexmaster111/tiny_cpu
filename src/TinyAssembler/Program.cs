// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using TinyAssemblerLib;
using OpCode = TinyCpuLib.OpCode;


var assemblyPaths = args
    .Where(File.Exists)
    .Where(x => Path.GetExtension(x) == ".casm")
    .ToArray();

var asmInputPath = assemblyPaths.FirstOrDefault();
if (asmInputPath == null)
{
    Console.WriteLine("No casm file given");
    return;
}

var outputPath = args.FirstOrDefault(x => x.StartsWith("-o"));
if (outputPath == null) outputPath = Path.GetFileNameWithoutExtension(asmInputPath) + ".hec";
else outputPath = outputPath[("-o".Length)..];
var tokenizer = new TinyAsmTokenizer(File.ReadAllLines(asmInputPath));
var tokens = tokenizer.Nom();
var asm = new TinyAsmAssembler(tokens);
var codeSectionBytes = asm.Assemble();
var hecFile = HecFile.New(codeSectionBytes);

var zip = codeSectionBytes.Zip(hecFile.TinyCpuCode);
#if DEBUG
bool fail = false;

Console.WriteLine("Verifying Assembly");
int byteNumber = 0;
foreach (var (f, s) in zip)
{
    if (!fail)
    {
        fail = f != s;
        if (fail) Console.WriteLine($"First mismatch @{byteNumber}");
    }

    if (f != s) Console.ForegroundColor = ConsoleColor.Red;
    Console.Write('.');
    if (f != s) Console.ResetColor();
    byteNumber++;
}

if (fail)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Assembly verify failed!");
}

Console.WriteLine("Assembly ok");

foreach (var inst in asm.AsmTokens)
{
    string instString = "";
    foreach (var b in inst.GetReadOnlyData())
    {
        instString += "0x" + b.ToString("X2") + ", ";
    }

    Console.WriteLine($"/*{asm.GetInstAddress(inst):x2}:*/ " +
                      instString +
                      $"// [{(OpCode)inst.GetReadOnlyData()[0]}] {inst.Token.Type} {inst.Token.ArgumentZeroData} {inst.Token.ArgumentOneData}");
}
#endif

File.WriteAllBytes(outputPath, hecFile.GetFileBytes());