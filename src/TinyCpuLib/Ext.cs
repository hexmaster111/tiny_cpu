namespace TinyCpuLib;

public static class Ext
{
    public static string MemoryDump(this int[] memory)
    {
        const int ADDR_PER_LINE = 8;
        var addr = 0;
        var o = "";
        foreach (var memVal in memory)
        {
            o += $"{addr++:X2}:{memVal:X4} ";
            if (addr % ADDR_PER_LINE == 0) o += Environment.NewLine;
        }

        return o;
    }

    public static int GetInstructionByteCount(this OpCode c) => c switch
    {
        OpCode.NOOP => 1, //Opcode
        OpCode.HALT => 1, //Opcode
        OpCode.RET => 1, //Opcode
        OpCode.CALLD => 1, //Opcode
        OpCode.SETREG_INTR_INTC => 1 + 1 + 4, //Opcode + byte + int
        OpCode.ADD_INTR_INTC => 1 + 1 + 4, //Opcode + byte + int
        OpCode.MUL_INTR_INTC => 1 + 1 + 4, //Opcode + byte + int
        OpCode.SUB_INTR_INTC => 1 + 1 + 4, //Opcode + byte + int
        OpCode.DIV_INTR_INTC => 1 + 1 + 4, //Opcode + byte + int
        OpCode.ADD_INTR_INTR => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.SETREG_INTR_INTR => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.MUL_INTR_INTR => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.SUB_INTR_INTR => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.DIV_INTR_INTR => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.CALL_INTC => 1 + 4, //Opcode + int
        OpCode.CALL_INTR => 1 + 1, //Opcode + byte
        OpCode.PUSH_INTC => 1 + 4, //Opcode + int
        OpCode.PUSH_INTR => 1 + 1, //Opcode + byte
        OpCode.POP_INTR => 1 + 1, //Opcode + byte
        OpCode.INC_INTR => 1 + 1, //Opcode + byte
        OpCode.DEC_INTR => 1 + 1, //Opcode + byte
        OpCode.CMP_INTR_INTC => 1 + 1 + 4, //Opcode + byte + int
        OpCode.CMP_INTR_INTR => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.JMP_INTC_EQ => 1 + 4, //Opcode + Int
        OpCode.JMP_INTC_NEQ => 1 + 4, //Opcode + Int
        OpCode.JMP_INTC_GTR => 1 + 4, //Opcode + Int
        OpCode.JMP_INTC_GEQ => 1 + 4, //Opcode + Int
        OpCode.JMP_INTC_LES => 1 + 4, //Opcode + Int
        OpCode.JMP_INTC_LEQ => 1 + 4, //Opcode + Int
        OpCode.JMP_INTR_EQ => 1 + 1, //Opcode + byte
        OpCode.JMP_INTR_NEQ => 1 + 1, //Opcode + byte
        OpCode.JMP_INTR_GTR => 1 + 1, //Opcode + byte
        OpCode.JMP_INTR_GEQ => 1 + 1, //Opcode + byte
        OpCode.JMP_INTR_LES => 1 + 1, //Opcode + byte
        OpCode.JMP_INTR_LEQ => 1 + 1, //Opcode + byte
        OpCode.JMP_INTR => 1 + 1, // Opcode byte
        OpCode.JMP_INTC => 1 + 4, //opcode int
        OpCode.MEM_READ_INTR_INTC => 1 + 1 + 4, //opcode byte int
        OpCode.MEM_READ_INTR_INTR => 1 + 1 + 1, //opcode byte byte
        OpCode.MEM_WRITE_INTR_INTC => 1 + 1 + 4, //opcode byte int
        OpCode.MEM_WRITE_INTR_INTR => 1 + 1 + 1, //opcode byte byte
        OpCode.SETREG_STRR_STRR => 1 + 1 + 1, //opcode byte byte
        OpCode.CCAT_STRR_STRR => 1 + 1 + 1, //opcode byte byte
        OpCode.CMP_STRR_STRR => 1 + 1 + 1, //opcode byte byte
        OpCode.STRSPLT_STRR_INTC => 1 + 1 + 4, //opcode byte int

        OpCode.SETREG_STRR_STRC => throw new InvalidOperationException(), //len changes
        OpCode.CCAT_STRR_STRC => throw new InvalidOperationException(), //len changes
        OpCode.CMP_STRR_STRC => throw new InvalidOperationException(), //len changes
        OpCode.PUSH_STRC => throw new InvalidOperationException(), //len changes
        
        OpCode.STRSPLT_STRR_INTR => 1 + 1 + 1, //opcode byte byte
        OpCode.MEM_READ_STRR_INTC => 1 + 1 + 4, //opcode byte int
        OpCode.MEM_READ_STRR_INTR => 1 + 1 + 1, //opcode byte byte
        OpCode.MEM_WRITE_STRR_INTC => 1 + 1 + 4, //opcode byte int
        OpCode.MEM_WRITE_STRR_INTR => 1 + 1 + 1, //opcode byte byte
        OpCode.PUSH_STRR => 1 + 1, //opcode byte
        OpCode.POP_STRR => 1 + 1, //opcode byte
        _ => throw new Exception($"Unknown OPCODE: {c}"),
    };

    public static bool ReadBit(this int flags, int bitIndex) => (flags & (1 << bitIndex)) != 0;
    public static bool ReadBit(this int flags, FLAGS_0_USAGE flag) => ReadBit(flags, (int)flag);

    public static void WriteBit(this ref int flags, FLAGS_0_USAGE flag, bool newValue) =>
        WriteBit(ref flags, (int)flag, newValue);

    public static void WriteBit(this ref int flags, int bitIndex, bool newValue)
    {
        if (newValue)
        {
            flags |= (1 << bitIndex);
        }
        else
        {
            flags &= ~(1 << bitIndex);
        }
    }
}