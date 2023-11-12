public static class Ext
{
    public static int ExpectedArgumetCount(this TinyAsmTokenizer.Token.TokenType t) => t switch
    {
        TinyAsmTokenizer.Token.TokenType.NOOP => 0,
        TinyAsmTokenizer.Token.TokenType.SETREG => 2,
        TinyAsmTokenizer.Token.TokenType.ADD => 2,
        TinyAsmTokenizer.Token.TokenType.SUB => 2,
        TinyAsmTokenizer.Token.TokenType.DIV => 2,
        TinyAsmTokenizer.Token.TokenType.MUL => 2,
        TinyAsmTokenizer.Token.TokenType.LBL => 1,
        TinyAsmTokenizer.Token.TokenType.CALL => 1,
        TinyAsmTokenizer.Token.TokenType.HALT => 0,
        TinyAsmTokenizer.Token.TokenType.RET => 0,
        TinyAsmTokenizer.Token.TokenType.NONE => throw new InvalidOperationException(),
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };
}