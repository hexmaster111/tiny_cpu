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
            sb.Append($"{reg}: {cpu.Reg[reg]:X4}");
        }

        sb.AppendLine();
        foreach (var reg in Enum.GetValues<StrRegisterIndex>())
        {
            sb.Append($"{reg}: {cpu.Reg[reg]}");
        }
        
        sb.AppendLine();
        foreach (var flags0Usage in Enum.GetValues<FLAGS_0_USAGE>())
        {
            sb.Append($"{flags0Usage}:{cpu.Reg.FLAGS_0.ReadBit((int)flags0Usage)} ");
        }

        sb.AppendLine();
        var currInst = (OpCode)cpu.ReadInstructionByteRel(0);
        sb.AppendLine($"Current Instruction :{currInst}");
        sb.AppendLine(cpu.Memory.Debugger_ReadAllIntMemoryAddresses().MemoryDump());
        return sb.ToString();
    }
}