﻿using System.Diagnostics;
using Newtonsoft.Json.Linq;
using TinyCpuLib;

namespace TinyCpu;

internal class Program
{
    private static TinyCpuLib.TinyCpu _cpu = new();

    private static void Main(string[] args)
    {
        _cpu.LoadProgram(new byte[]
        {
/*00:*/ 0xA6, // [CALLD] LBL MAIN 
/*01:*/ 0xC0, 0x00, 0x53, 0x4F, 0x4D, 0x45, 0x20, 0x54, 0x48, 0x49, 0x4E, 0x47, 0x00, // [SETREG_STRR_STRC] SETREG GP_STR_0 SOME THING
/*0e:*/ 0xC0, 0x01, 0x55, 0x53, 0x45, 0x52, 0x20, 0x48, 0x41, 0x49, 0x4C, 0x45, 0x59, 0x20, 0x53, 0x41, 0x59, 0x53, 0x20, 0x48, 0x45, 0x4C, 0x4C, 0x4F, 0x20, 0x57, 0x4F, 0x52, 0x4C, 0x44, 0x21, 0x00, // [SETREG_STRR_STRC] SETREG GP_STR_1 USER HAILEY SAYS HELLO WORLD!
/*2e:*/ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // [NOOP] SSPLIT GP_STR_1 4
/*34:*/ 0xC5, 0x01, 0x55, 0x53, 0x45, 0x52, 0x00, // [CMP_STRR_STRC] CMP GP_STR_1 USER
/*3b:*/ 0xA7, 0x6D, 0x00, 0x00, 0x00, // [JMP_INTC_EQ] JMP_EQ PARSE_USER 
/*40:*/ 0xC5, 0x01, 0x46, 0x55, 0x4E, 0x43, 0x00, // [CMP_STRR_STRC] CMP GP_STR_1 FUNC
/*47:*/ 0xA7, 0x7D, 0x00, 0x00, 0x00, // [JMP_INTC_EQ] JMP_EQ PARSE_FUNC 
/*4c:*/ 0xC5, 0x01, 0x44, 0x41, 0x54, 0x41, 0x00, // [CMP_STRR_STRC] CMP GP_STR_1 DATA
/*53:*/ 0xA7, 0x83, 0x00, 0x00, 0x00, // [JMP_INTC_EQ] JMP_EQ PARSE_DATA 
/*58:*/ 0xCD, 0x49, 0x49, 0x4E, 0x56, 0x41, 0x4C, 0x49, 0x44, 0x20, 0x41, 0x52, 0x47, 0x00, // [PUSH_STRC] PUSH INVALID ARG! 
/*66:*/ 0xB4, 0x89, 0x00, 0x00, 0x00, // [JMP_INTC] JMP ERROR_FUNC 
/*6b:*/ 0xA6, // [CALLD] LBL PARSE_DONE 
/*6c:*/ 0xFF, // [HALT] HALT  
/*6d:*/ 0xA6, // [CALLD] LBL PARSE_USER 
/*6e:*/ 0xC0, 0x00, 0x20, 0x00, // [SETREG_STRR_STRC] SETREG GP_STR_0  
/*72:*/ 0xB5, 0x01, 0x00, 0x00, 0x00, 0x00, // [MEM_READ_INTR_INTC] MEM_READ GP_STR_1 0
/*78:*/ 0xB4, 0x6B, 0x00, 0x00, 0x00, // [JMP_INTC] JMP PARSE_DONE 
/*7d:*/ 0xA6, // [CALLD] LBL PARSE_FUNC 
/*7e:*/ 0xB4, 0x6B, 0x00, 0x00, 0x00, // [JMP_INTC] JMP PARSE_DONE 
/*83:*/ 0xA6, // [CALLD] LBL PARSE_DATA 
/*84:*/ 0xB4, 0x6B, 0x00, 0x00, 0x00, // [JMP_INTC] JMP PARSE_DONE 
/*89:*/ 0xA6, // [CALLD] LBL ERROR_FUNC 
/*8a:*/ 0xCE, 0x01, // [POP_STRR] POP GP_STR_1 
/*8c:*/ 0xFF, // [HALT] HALT  
        });

        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
        {
            Console.WriteLine(TinyCpuVisualisation.DumpState(_cpu));
            Console.ReadKey();
            _cpu.Step();
        }

        Console.WriteLine("CPU Halted");
    }
}