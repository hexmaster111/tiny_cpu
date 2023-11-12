// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Globalization;
using TinyAssembler;

var programSectionFileLines = new string[]
{
    "CALL CALL_DEMO ;calls out to set a register to 69",
    "CALL 0x00      ;Just a test to make sure we can call constant typed values",
    "CALL GP_I32_0  ;Just a test",
    "HALT           ;halt the cpu",
    "LBL CALL_DEMO",
    ";comment line",
    "SETREG GP_I32_0 0x69",
    "RET",
};


var tokenizer = new TinyAsmTokenizer(programSectionFileLines);
var tokens = tokenizer.Nom();
var asm = new TinyAsmAssembler(tokens);
var asmBytes = asm.Assemble();
Debugger.Break();


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