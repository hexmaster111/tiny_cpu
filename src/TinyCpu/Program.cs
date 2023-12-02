using System.Diagnostics;
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
            /*00:*/ 0x01, 0x05, 0x01, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_1 1
            /*06:*/ 0xB7, 0x05, 0x01, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_1 1
            /*0c:*/ 0xB8, 0x05, 0x04, // [MEM_WRITE_R_R] MEM_WRITE GP_I32_1 GP_I32_0
            /*0f:*/ 0xB5, 0x05, 0x01, 0x00, 0x00, 0x00, // [MEM_READ_R_C] MEM_READ GP_I32_1 1
            /*15:*/ 0xFF, // [HALT] HALT  
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