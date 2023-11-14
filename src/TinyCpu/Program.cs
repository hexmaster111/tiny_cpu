using Newtonsoft.Json.Linq;
using TinyCpuLib;

namespace TinyCpu;

internal class Program
{
    static TinyCpuLib.TinyCpu _cpu = new();

    private static void Main(string[] args)
    {
        _cpu.LoadProgram(new byte[]
        {
            /*00:*/ 0xA3, 0x30, 0x00, 0x00, 0x00,       // [CALL_C] CALL CALL_DEMO 
            /*05:*/ 0xA6,                               // [CALL_D] LBL MORE_MATH 
            /*06:*/ 0x03, 0x04, 0x04, 0x00, 0x00, 0x00, // [ADD_R_C] ADD GP_I32_0 4
            /*0c:*/ 0x07, 0x04, 0x01, 0x00, 0x00, 0x00, // [SUB_R_C] SUB GP_I32_0 1
            /*12:*/ 0x09, 0x04, 0x01, 0x00, 0x00, 0x00, // [DIV_R_C] DIV GP_I32_0 1
            /*18:*/ 0x05, 0x04, 0x04, 0x00, 0x00, 0x00, // [MUL_R_C] MUL GP_I32_0 4
            /*1e:*/ 0x0B, 0x05,                         // [INC] INC GP_I32_1 
            /*20:*/ 0x0D, 0x05, 0x05, 0x00, 0x00, 0x00, // [CMP_R_C] CMP GP_I32_1 5
            /*26:*/ 0xA7, 0x3F, 0x00, 0x00, 0x00,       // [JMP_C_EQ] JMP_EQ END
            /*2b:*/ 0xB4, 0x05, 0x00, 0x00, 0x00,       // [JMP_C] JMP MORE_MATH
            /*30:*/ 0xA6,                               // [CALL_D] LBL CALL_DEMO
            /*31:*/ 0x01, 0x04, 0xFF, 0xFF, 0xFF, 0xFF, // [SETREG_R_C] SETREG GP_I32_0 FFFFFFFF
            /*37:*/ 0xA0, 0xFF, 0x00, 0x00, 0x00,       // [PUSH_C] PUSH FF
            /*3c:*/ 0xA1, 0x04,                         // [PUSH_R] PUSH GP_I32_0
            /*3e:*/ 0xA5,                               // [RET] RET
            /*3f:*/ 0xA6,                               // [CALL_D] LBL END
            /*40:*/ 0xFF,                               // [HALT] HALT
        });

        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
        {
            _cpu.DumpState();
            _cpu.Step();
        }
    }
}