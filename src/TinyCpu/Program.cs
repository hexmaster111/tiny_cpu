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
            /*00:*/ 0xA3, 0x1E, 0x00, 0x00, 0x00, // [CALL_C] CALL CALL_DEMO 
            /*05:*/ 0x03, 0x04, 0x04, 0x00, 0x00, 0x00, // [ADD_R_C] ADD GP_I32_0 4
            /*0b:*/ 0x07, 0x04, 0x01, 0x00, 0x00, 0x00, // [SUB_R_C] SUB GP_I32_0 1
            /*11:*/ 0x09, 0x04, 0x01, 0x00, 0x00, 0x00, // [DIV_R_C] DIV GP_I32_0 1
            /*17:*/ 0x05, 0x04, 0x04, 0x00, 0x00, 0x00, // [MUL_R_C] MUL GP_I32_0 4
            /*1d:*/ 0xFF, // [HALT] HALT  
            /*1e:*/ 0xA6, // [CALL_D] LBL CALL_DEMO 
            /*1f:*/ 0x01, 0x69, 0x00, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_0 69
            0xA0, 0x50, 0x50, 0x00, 0x00, //PUSH CONST
            0xA1, 
            /*25:*/ 0xA5, // [RET] RET 
        });

        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.F0_HALT))
        {
            _cpu.DumpState();
            _cpu.Step();
        }
    }
}