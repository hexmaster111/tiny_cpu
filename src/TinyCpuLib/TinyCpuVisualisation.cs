using System.Text;

namespace TinyCpuLib;

public static class TinyCpuVisualisation
{
    public static string DumpState(TinyCpu cpu)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"---TICK {cpu.Cycles:00000000}-----");
        sb.AppendLine($"{nameof(CpuRegisters.INST_PTR)}:{cpu.Reg.INST_PTR:X4} ");

        foreach (var reg in Enum.GetValues<IntRegisterIndex>())
        {
            sb.AppendLine($"{reg}: {cpu.Reg[reg]:X4}");
        }

        foreach (var flags0Usage in Enum.GetValues<FLAGS_0_USAGE>())
        {
            sb.AppendLine($"{flags0Usage}:{cpu.Reg.FLAGS_0.ReadBit((int)flags0Usage)}");
        }

        var currInst = (OpCode)cpu.ReadInstructionByteRel(0);
        sb.AppendLine($"Current Instruction :{currInst}");
        sb.AppendLine(cpu.Memory.Debugger_ReadAllMemoryAddresses().MemoryDump());
        return sb.ToString();
    }
}