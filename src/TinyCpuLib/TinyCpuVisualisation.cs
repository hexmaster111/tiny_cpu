using System.Text;

namespace TinyCpuLib;

public static class TinyCpuVisualisation
{
    public static string DumpState(TinyCpu cpu)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"---TICK {cpu.Cycles:00000000}-----");
        sb.AppendLine($"{nameof(CpuRegisters.INST_PTR)}:{cpu.Reg.INST_PTR:X4} ");
        sb.AppendLine($"{nameof(CpuRegisters.GP_I32_0)}:{cpu.Reg.GP_I32_0:X4} " +
                      $"{nameof(CpuRegisters.GP_I32_1)}:{cpu.Reg.GP_I32_1:X4} " +
                      $"{nameof(CpuRegisters.GP_I32_2)}:{cpu.Reg.GP_I32_2:X4}\n" +
                      $"{nameof(CpuRegisters.FLAGS_0)}: " +
                      $"{nameof(FLAGS_0_USAGE.HALT)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT)} " +
                      $"{nameof(FLAGS_0_USAGE.EQ)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.EQ)} " +
                      $"{nameof(FLAGS_0_USAGE.NEQ)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.NEQ)} " +
                      $"{nameof(FLAGS_0_USAGE.GTR)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GTR)} " +
                      $"{nameof(FLAGS_0_USAGE.GEQ)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GEQ)} " +
                      $"{nameof(FLAGS_0_USAGE.LES)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LES)} " +
                      $"{nameof(FLAGS_0_USAGE.LEQ)}:{cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LEQ)}");
        var currInst = (OpCode)cpu.ReadInstructionByteRel(0);
        sb.AppendLine($"Current Instruction :{currInst}");
        sb.AppendLine(cpu.Memory.Debugger_ReadAllMemoryAddresses().MemoryDump());
        return sb.ToString();
    }
}