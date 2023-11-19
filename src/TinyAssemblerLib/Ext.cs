using static TinyAssemblerLib.TinyAsmTokenizer.Token;

namespace TinyAssemblerLib;

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
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };
}