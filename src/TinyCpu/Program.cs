using TinyCpuLib;

namespace TinyCpu;

internal class Program
{
    static TinyCpuLib.TinyCpu _cpu = new();

    private static void Main(string[] args)
    {
        _cpu.LoadProgram(new byte[]
        {
            /*00:*/ 0xA3, 0x06, 0x00, 0x00, 0x00, //CALL CALL_DEMO
            /*05:*/ 0xFF, //HALT
            /*06:*/ 0xA6, //LBL CALL_DEMO
            /*07:*/ 0xA5, //RET
        });

        while (!_cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.F0_HALT))
        {
            _cpu.DumpState();
            _cpu.Step();
        }
    }
}