

public static class OpCodeExt
{
    public static int GetInstructionByteCount(this OpCode c) => c switch
    {
        OpCode.NOOP => 1, //Opcode
        OpCode.SETREG_R_C => 3, //Opcode + 1 byte arg + 1 byte arg
        _ => throw new Exception($"Unknown OPCODE: {c}"),
    };
}
