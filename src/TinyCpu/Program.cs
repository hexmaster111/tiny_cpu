using TinyCpuLib;

namespace TinyCpu;

internal class Program
{
    static TinyCpuLib.TinyCpu _cpu = new();

    private static void Main(string[] args)
    {
        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.F0_HALT))
        {
            _cpu.DumpState();
            _cpu.Step();
        }
    }
}