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
            0x01, 0x04, 0x01, 0x00, 0x00, 0x00, //Set register GP_1 to const 1
            0xB7, 0x04, 0x01, 0x00, 0x00, 0x00, // Write the value in GP_1 to memory address 0x01
            0xFF //HALT
        });

        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
        {
            _cpu.DumpState();
            Console.ReadKey();
            _cpu.Step();
        }

        Console.WriteLine("CPU Halted");
    }
}