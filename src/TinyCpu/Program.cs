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
            192, 0, // set str reg 0 to
            32, 72, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 0, //END OF STRING
            193, 1, 0, // set str reg 1 to reg 0
            194, 1, 0, // concat the two strings
            196, 1, 0, // Compare the two strings
            0xC5, 0x01, 72, 101, 108, 108, 111, 0x00, // comp "Hello" to str reg 1
            192, 0, // set str reg 0 to
            32, 0, //END OF STRING
            0xC6, 0x01, 0x01, 0x00, 0x00, 0x00, // STR SPLIT reg 1, take part 1, save to reg 1
            0xCD, 72, 101, 108, 108, 111, 32, 119, 111, 114, 108, 100, 0x00, // push str "Hello World" to stack
            255, // [HALT] HALT  
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