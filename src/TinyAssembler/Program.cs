// See https://aka.ms/new-console-template for more information

using TinyAssemblerLib;
using OpCode = TinyCpuLib.OpCode;

var programSectionFileLines = new string[]
{
    "setreg gp_i32_1 0x01",
    "mem_write gp_i32_1 0x01",
    "mem_write gp_i32_1 gp_i32_0",
    "mem_read gp_i32_1 0x01 ; read the value from 0x01 into reg",
    "HALT"
};


var tokenizer = new TinyAsmTokenizer(programSectionFileLines);
var tokens = tokenizer.Nom();
var asm = new TinyAsmAssembler(tokens);
var asmBytes = asm.Assemble();


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