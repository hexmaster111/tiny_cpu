// See https://aka.ms/new-console-template for more information

using TinyAssembler;
using OpCode = TinyCpuLib.OpCode;

var programSectionFileLines = new string[]
{
    "CALL CALL_DEMO",
    "NOOP",
    "LBL MORE_MATH",
    "INC GP_I32_1",
    "CMP GP_I32_1 0x5",
    "JMP_EQ END",
    "JMP MORE_MATH",
    "NOOP", "NOOP", "NOOP", "NOOP", "NOOP", "NOOP",
    "LBL CALL_DEMO",
    "NOOP ;APP CODE",
    "RET",
    "LBL END",
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


/*
Instructions:
NOOP
SETREG [SRC<REG{gp_0, gp_1}/CONST{int}>]  [DEST<REG/CONST>]
ADD [SRC<REG>] [DEST<REG/CONST>]
SUB [SRC<REG>] [DEST<REG/CONST>]
DIV [SRC<REG>] [DEST<REG/CONST>]
MUL [SRC<REG>] [DEST<REG/CONST>]
CALL [STR<LABEL>]
RET
LBL [STR<LABEL_NAME>]
HALT

---- SYNTAX
; comment
all instructions one line
 */