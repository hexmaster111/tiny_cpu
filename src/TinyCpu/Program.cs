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
            /*00:*/ 0xA3, 0x1F, 0x00, 0x00, 0x00, // [CALL_C] CALL CALL_DEMO
            /*05:*/ 0x00, // [NOOP] NOOP
            /*06:*/ 0xA6, // [CALL_D] LBL MORE_MATH
            /*07:*/ 0x0B, 0x05, // [INC] INC GP_I32_1
            /*09:*/ 0x0D, 0x05, 0x05, 0x00, 0x00, 0x00, // [CMP_R_C] CMP GP_I32_1 5
            /*0f:*/ 0xA7, 0x22, 0x00, 0x00, 0x00, // [JMP_C_EQ] JMP_EQ END
            /*14:*/ 0xB4, 0x06, 0x00, 0x00, 0x00, // [JMP_C] JMP MORE_MATH
            /*19:*/ 0x00, // [NOOP] NOOP
            /*1a:*/ 0x00, // [NOOP] NOOP
            /*1b:*/ 0x00, // [NOOP] NOOP
            /*1c:*/ 0x00, // [NOOP] NOOP
            /*1d:*/ 0x00, // [NOOP] NOOP
            /*1e:*/ 0x00, // [NOOP] NOOP
            /*1f:*/ 0xA6, // [CALL_D] LBL CALL_DEMO
            /*20:*/ 0x00, // [NOOP] NOOP
            /*21:*/ 0xA5, // [RET] RET
            /*22:*/ 0xA6, // [CALL_D] LBL END
            /*23:*/ 0xFF, // [HALT] HALT
        });

        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
        {
            _cpu.DumpState();
            //Console.ReadKey();
            _cpu.Step();
        }

        Console.WriteLine("CPU Halted");
    }
}