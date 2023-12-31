using System.Text;
using TinyCpuLib;
using static TinyAssemblerLib.TinyAsmTokenizer.Token;

namespace TinyExt;

public static class Ext
{
    
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
        TokenType.SCCAT => 2,
        TokenType.SSPLIT => 2,
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };

    public static string FromBytes(this ArgumentType @this, byte[] argData)
    {
        return @this switch
        {
            ArgumentType.ConstInt => BitConverter.ToInt32(argData).ToString(),
            ArgumentType.IntRegister => ((IntRegisterIndex)argData[0]).ToString(),
            ArgumentType.NONE => "",
            ArgumentType.FuncName => throw new InvalidOperationException(),
            ArgumentType.StrRegister => ((StrRegisterIndex)argData[0]).ToString(),
            ArgumentType.StrLiteral => Encoding.ASCII.GetString(argData),
            _ => throw new ArgumentOutOfRangeException(nameof(@this), @this, null)
        };
    }
}