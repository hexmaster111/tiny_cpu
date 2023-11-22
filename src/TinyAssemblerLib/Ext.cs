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
        OpCode.INC => ArgumentType.NONE,
        OpCode.DEC => ArgumentType.NONE,
        OpCode.CALL_C => ArgumentType.NONE,
        OpCode.CALL_R => ArgumentType.NONE,
        OpCode.RET => ArgumentType.NONE,
        OpCode.CALL_D => ArgumentType.NONE,
        OpCode.JMP_C_EQ => ArgumentType.NONE,
        OpCode.JMP_C_NEQ => ArgumentType.NONE,
        OpCode.JMP_C_GTR => ArgumentType.NONE,
        OpCode.JMP_C_GEQ => ArgumentType.NONE,
        OpCode.JMP_C_LES => ArgumentType.NONE,
        OpCode.JMP_C_LEQ => ArgumentType.NONE,
        OpCode.JMP_R_EQ => ArgumentType.NONE,
        OpCode.JMP_R_NEQ => ArgumentType.NONE,
        OpCode.JMP_R_GTR => ArgumentType.NONE,
        OpCode.JMP_R_GEQ => ArgumentType.NONE,
        OpCode.JMP_R_LES => ArgumentType.NONE,
        OpCode.JMP_R_LEQ => ArgumentType.NONE,
        OpCode.JMP_R => ArgumentType.NONE,
        OpCode.JMP_C => ArgumentType.NONE,
        OpCode.PUSH_C => ArgumentType.NONE,
        OpCode.PUSH_R => ArgumentType.NONE,
        OpCode.POP_R => ArgumentType.NONE,
        OpCode.HALT => ArgumentType.NONE,

        OpCode.SETREG_R_C => ArgumentType.CONST,
        OpCode.ADD_R_C => ArgumentType.CONST,
        OpCode.MUL_R_C => ArgumentType.CONST,
        OpCode.DIV_R_C => ArgumentType.CONST,
        OpCode.SUB_R_C => ArgumentType.CONST,
        OpCode.MEM_READ_R_C => ArgumentType.CONST,
        OpCode.MEM_WRITE_R_C => ArgumentType.CONST,

        OpCode.SETREG_R_R => ArgumentType.REGISTER,
        OpCode.ADD_R_R => ArgumentType.REGISTER,
        OpCode.MUL_R_R => ArgumentType.REGISTER,
        OpCode.SUB_R_R => ArgumentType.REGISTER,
        OpCode.DIV_R_R => ArgumentType.REGISTER,
        OpCode.CMP_R_C => ArgumentType.REGISTER,
        OpCode.CMP_R_R => ArgumentType.REGISTER,
        OpCode.MEM_READ_R_R => ArgumentType.REGISTER,
        OpCode.MEM_WRITE_R_R => ArgumentType.REGISTER,
        _ => throw new ArgumentOutOfRangeException(nameof(oc), oc, null)
    };

    public static ArgumentType ArgOneType(this OpCode oc) => oc switch
    {
        OpCode.NOOP => ArgumentType.NONE,
        OpCode.RET => ArgumentType.NONE,
        OpCode.CALL_D => ArgumentType.NONE,
        OpCode.HALT => ArgumentType.NONE,

        OpCode.MEM_READ_R_C => ArgumentType.REGISTER,
        OpCode.MEM_READ_R_R => ArgumentType.REGISTER,
        OpCode.MEM_WRITE_R_C => ArgumentType.REGISTER,
        OpCode.MEM_WRITE_R_R => ArgumentType.REGISTER,
        OpCode.SETREG_R_C => ArgumentType.REGISTER,
        OpCode.SETREG_R_R => ArgumentType.REGISTER,
        OpCode.ADD_R_C => ArgumentType.REGISTER,
        OpCode.ADD_R_R => ArgumentType.REGISTER,
        OpCode.MUL_R_C => ArgumentType.REGISTER,
        OpCode.MUL_R_R => ArgumentType.REGISTER,
        OpCode.SUB_R_C => ArgumentType.REGISTER,
        OpCode.SUB_R_R => ArgumentType.REGISTER,
        OpCode.DIV_R_C => ArgumentType.REGISTER,
        OpCode.DIV_R_R => ArgumentType.REGISTER,
        OpCode.INC => ArgumentType.REGISTER,
        OpCode.DEC => ArgumentType.REGISTER,
        OpCode.CMP_R_C => ArgumentType.REGISTER,
        OpCode.CMP_R_R => ArgumentType.REGISTER,
        OpCode.PUSH_R => ArgumentType.REGISTER,
        OpCode.POP_R => ArgumentType.REGISTER,
        OpCode.CALL_R => ArgumentType.REGISTER,
        OpCode.JMP_R_EQ => ArgumentType.REGISTER,
        OpCode.JMP_R_NEQ => ArgumentType.REGISTER,
        OpCode.JMP_R_GTR => ArgumentType.REGISTER,
        OpCode.JMP_R_GEQ => ArgumentType.REGISTER,
        OpCode.JMP_R_LES => ArgumentType.REGISTER,
        OpCode.JMP_R_LEQ => ArgumentType.REGISTER,
        OpCode.JMP_R => ArgumentType.REGISTER,

        OpCode.PUSH_C => ArgumentType.CONST,
        OpCode.CALL_C => ArgumentType.CONST,
        OpCode.JMP_C_EQ => ArgumentType.CONST,
        OpCode.JMP_C_NEQ => ArgumentType.CONST,
        OpCode.JMP_C_GTR => ArgumentType.CONST,
        OpCode.JMP_C_GEQ => ArgumentType.CONST,
        OpCode.JMP_C_LES => ArgumentType.CONST,
        OpCode.JMP_C_LEQ => ArgumentType.CONST,
        OpCode.JMP_C => ArgumentType.CONST,
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
            ArgumentType.REGISTER => ((RegisterIndex)argData[0]).ToString(),
            ArgumentType.NONE => "",
            ArgumentType.STR => throw new InvalidOperationException(),
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
        };
    }
}