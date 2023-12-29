using TinyCpuLib;
using static TinyAssemblerLib.TinyAsmTokenizer.Token;

namespace TinyExt;

public static class Ext
{
    public static int ByteCount(this ArgumentType t) => t switch
    {
        ArgumentType.CONST => sizeof(int),
        ArgumentType.REGISTER => sizeof(byte),
        ArgumentType.NONE => 0,
        ArgumentType.STR => throw new InvalidOperationException("str"),
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };

    public static ArgumentType ArgTwoType(this OpCode oc) => oc switch
    {
        OpCode.NOOP => ArgumentType.NONE,
        OpCode.INC_INTR => ArgumentType.NONE,
        OpCode.DEC_INTR => ArgumentType.NONE,
        OpCode.CALL_INTC => ArgumentType.NONE,
        OpCode.CALL_INTR => ArgumentType.NONE,
        OpCode.RET => ArgumentType.NONE,
        OpCode.CALLD => ArgumentType.NONE,
        OpCode.JMP_INTC_EQ => ArgumentType.NONE,
        OpCode.JMP_INTC_NEQ => ArgumentType.NONE,
        OpCode.JMP_INTC_GTR => ArgumentType.NONE,
        OpCode.JMP_INTC_GEQ => ArgumentType.NONE,
        OpCode.JMP_INTC_LES => ArgumentType.NONE,
        OpCode.JMP_INTC_LEQ => ArgumentType.NONE,
        OpCode.JMP_INTR_EQ => ArgumentType.NONE,
        OpCode.JMP_INTR_NEQ => ArgumentType.NONE,
        OpCode.JMP_INTR_GTR => ArgumentType.NONE,
        OpCode.JMP_INTR_GEQ => ArgumentType.NONE,
        OpCode.JMP_INTR_LES => ArgumentType.NONE,
        OpCode.JMP_INTR_LEQ => ArgumentType.NONE,
        OpCode.JMP_INTR => ArgumentType.NONE,
        OpCode.JMP_INTC => ArgumentType.NONE,
        OpCode.PUSH_INTC => ArgumentType.NONE,
        OpCode.PUSH_INTR => ArgumentType.NONE,
        OpCode.POP_INTR => ArgumentType.NONE,
        OpCode.HALT => ArgumentType.NONE,

        OpCode.SETREG_INTR_INTC => ArgumentType.CONST,
        OpCode.ADD_INTR_INTC => ArgumentType.CONST,
        OpCode.MUL_INTR_INTC => ArgumentType.CONST,
        OpCode.DIV_INTR_INTC => ArgumentType.CONST,
        OpCode.SUB_INTR_INTC => ArgumentType.CONST,
        OpCode.MEM_READ_INTR_INTC => ArgumentType.CONST,
        OpCode.MEM_WRITE_INTR_INTC => ArgumentType.CONST,

        OpCode.SETREG_INTR_INTR => ArgumentType.REGISTER,
        OpCode.ADD_INTR_INTR => ArgumentType.REGISTER,
        OpCode.MUL_INTR_INTR => ArgumentType.REGISTER,
        OpCode.SUB_INTR_INTR => ArgumentType.REGISTER,
        OpCode.DIV_INTR_INTR => ArgumentType.REGISTER,
        OpCode.CMP_INTR_INTC => ArgumentType.REGISTER,
        OpCode.CMP_INTR_INTR => ArgumentType.REGISTER,
        OpCode.MEM_READ_INTR_INTR => ArgumentType.REGISTER,
        OpCode.MEM_WRITE_INTR_INTR => ArgumentType.REGISTER,
        _ => throw new ArgumentOutOfRangeException(nameof(oc), oc, null)
    };

    public static ArgumentType ArgOneType(this OpCode oc) => oc switch
    {
        OpCode.NOOP => ArgumentType.NONE,
        OpCode.RET => ArgumentType.NONE,
        OpCode.CALLD => ArgumentType.NONE,
        OpCode.HALT => ArgumentType.NONE,

        OpCode.MEM_READ_INTR_INTC => ArgumentType.REGISTER,
        OpCode.MEM_READ_INTR_INTR => ArgumentType.REGISTER,
        OpCode.MEM_WRITE_INTR_INTC => ArgumentType.REGISTER,
        OpCode.MEM_WRITE_INTR_INTR => ArgumentType.REGISTER,
        OpCode.SETREG_INTR_INTC => ArgumentType.REGISTER,
        OpCode.SETREG_INTR_INTR => ArgumentType.REGISTER,
        OpCode.ADD_INTR_INTC => ArgumentType.REGISTER,
        OpCode.ADD_INTR_INTR => ArgumentType.REGISTER,
        OpCode.MUL_INTR_INTC => ArgumentType.REGISTER,
        OpCode.MUL_INTR_INTR => ArgumentType.REGISTER,
        OpCode.SUB_INTR_INTC => ArgumentType.REGISTER,
        OpCode.SUB_INTR_INTR => ArgumentType.REGISTER,
        OpCode.DIV_INTR_INTC => ArgumentType.REGISTER,
        OpCode.DIV_INTR_INTR => ArgumentType.REGISTER,
        OpCode.INC_INTR => ArgumentType.REGISTER,
        OpCode.DEC_INTR => ArgumentType.REGISTER,
        OpCode.CMP_INTR_INTC => ArgumentType.REGISTER,
        OpCode.CMP_INTR_INTR => ArgumentType.REGISTER,
        OpCode.PUSH_INTR => ArgumentType.REGISTER,
        OpCode.POP_INTR => ArgumentType.REGISTER,
        OpCode.CALL_INTR => ArgumentType.REGISTER,
        OpCode.JMP_INTR_EQ => ArgumentType.REGISTER,
        OpCode.JMP_INTR_NEQ => ArgumentType.REGISTER,
        OpCode.JMP_INTR_GTR => ArgumentType.REGISTER,
        OpCode.JMP_INTR_GEQ => ArgumentType.REGISTER,
        OpCode.JMP_INTR_LES => ArgumentType.REGISTER,
        OpCode.JMP_INTR_LEQ => ArgumentType.REGISTER,
        OpCode.JMP_INTR => ArgumentType.REGISTER,

        OpCode.PUSH_INTC => ArgumentType.CONST,
        OpCode.CALL_INTC => ArgumentType.CONST,
        OpCode.JMP_INTC_EQ => ArgumentType.CONST,
        OpCode.JMP_INTC_NEQ => ArgumentType.CONST,
        OpCode.JMP_INTC_GTR => ArgumentType.CONST,
        OpCode.JMP_INTC_GEQ => ArgumentType.CONST,
        OpCode.JMP_INTC_LES => ArgumentType.CONST,
        OpCode.JMP_INTC_LEQ => ArgumentType.CONST,
        OpCode.JMP_INTC => ArgumentType.CONST,
        _ => throw new ArgumentOutOfRangeException(nameof(oc), oc, null)
    };


    public static int ExpectedArgumentCount(this TokenType t) => t switch
    {
        TokenType.NOOP => 0,
        TokenType.SETREG => 2,
        TokenType.ADD => 2,
        TokenType.SUB => 2,
        TokenType.DIV => 2,
        TokenType.MUL => 2,
        TokenType.LBL => 1,
        TokenType.CALL => 1,
        TokenType.HALT => 0,
        TokenType.RET => 0,
        TokenType.PUSH => 1,
        TokenType.POP => 1,
        TokenType.INC => 1,
        TokenType.DEC => 1,
        TokenType.CMP => 2,
        TokenType.NONE => throw new InvalidOperationException(),
        TokenType.JMP_EQ => 1,
        TokenType.JMP_NEQ => 1,
        TokenType.JMP_GTR => 1,
        TokenType.JMP_GEQ => 1,
        TokenType.JMP_LES => 1,
        TokenType.JMP_LEQ => 1,
        TokenType.JMP => 1,
        TokenType.MEM_READ => 2,
        TokenType.MEM_WRITE => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };

    public static string FromBytes(this ArgumentType @this, byte[] argData)
    {
        return @this switch
        {
            ArgumentType.CONST => BitConverter.ToInt32(argData).ToString(),
            ArgumentType.REGISTER => ((IntRegisterIndex)argData[0]).ToString(),
            ArgumentType.NONE => "",
            ArgumentType.STR => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
        };
    }
}