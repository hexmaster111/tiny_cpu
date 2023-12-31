using System.Collections.Immutable;
using System.Text;
using TinyCpuLib;

namespace TinyAssemblerLib;

public class AsmToken : IFreezable
{
    public override string ToString()
    {
        return $"{Token.Type} {Token.ArgumentZeroData} {Token.ArgumentOneData} ";
    }

    public TinyAsmTokenizer.Token Token { get; init; }

    private byte[] _data;

    public byte[] GetData()
    {
        if (IsFrozen) throw new Exception("I said this was done!");
        return _data;
    }

    private void SetData(byte[] value)
    {
        if (IsFrozen) throw new Exception("I said this was done!");
        _data = value;
    }

    private AsmToken(TinyAsmTokenizer.Token Token, byte[] Data)
    {
        this.Token = Token;
        this.SetData(Data);
    }

    public static AsmToken New(TinyAsmTokenizer.Token token) => token.Type switch
    {
        TinyAsmTokenizer.Token.TokenType.NOOP => new AsmToken(token, new[] { (byte)OpCode.NOOP }),
        TinyAsmTokenizer.Token.TokenType.LBL => new AsmToken(token, new[] { (byte)OpCode.CALLD }),
        TinyAsmTokenizer.Token.TokenType.HALT => new AsmToken(token, new[] { (byte)OpCode.HALT }),
        TinyAsmTokenizer.Token.TokenType.RET => new AsmToken(token, new[] { (byte)OpCode.RET }),
        TinyAsmTokenizer.Token.TokenType.NONE => throw new ArgumentOutOfRangeException(),
        _ => new AsmToken(token, new byte[GetTokenSize(token)]),
    };

    private static int GetTokenSize(TinyAsmTokenizer.Token token)
    {
        var argZeroSize = GetArgSize(token.ArgumentZeroType, token.ArgumentZeroData, token);
        var argOneSize = GetArgSize(token.ArgumentOneType, token.ArgumentOneData, token);

        return argZeroSize + argOneSize + 1; //+1 for opcode
    }

    private static int GetArgSize(
        TinyAsmTokenizer.Token.ArgumentType argType,
        string argData,
        TinyAsmTokenizer.Token parentToken
    ) => argType switch
    {
        TinyAsmTokenizer.Token.ArgumentType.ConstInt => 4,
        TinyAsmTokenizer.Token.ArgumentType.IntRegister => 1,
        TinyAsmTokenizer.Token.ArgumentType.FuncName => parentToken.Type switch
        {
            TinyAsmTokenizer.Token.TokenType.LBL => 0, //label just turns into a CALLDST and thats just the opcode
            TinyAsmTokenizer.Token.TokenType.CALL => 4, //INT address
            TinyAsmTokenizer.Token.TokenType.JMP_EQ => 4, //INT Address
            TinyAsmTokenizer.Token.TokenType.JMP_NEQ => 4, //INT Address
            TinyAsmTokenizer.Token.TokenType.JMP_GTR => 4, //INT Address
            TinyAsmTokenizer.Token.TokenType.JMP_GEQ => 4, //INT Address
            TinyAsmTokenizer.Token.TokenType.JMP_LES => 4, //INT Address
            TinyAsmTokenizer.Token.TokenType.JMP_LEQ => 4, //INT Address
            TinyAsmTokenizer.Token.TokenType.JMP => 4, //INT Address
            var v => throw new ArgumentOutOfRangeException($"{parentToken}")
        },
        TinyAsmTokenizer.Token.ArgumentType.NONE => 0,
        TinyAsmTokenizer.Token.ArgumentType.StrRegister => 1, //1 byte for register
        TinyAsmTokenizer.Token.ArgumentType.StrLiteral => Encoding.ASCII.GetBytes(argData).Length + 1, //+1 for null
        _ => throw new ArgumentOutOfRangeException(nameof(argType), argType, null)
    };


    public void Deconstruct(out TinyAsmTokenizer.Token Token, out byte[] Data)
    {
        Token = this.Token;
        Data = this.GetData();
    }

    public bool IsFrozen { get; private set; }
    public void Freeze() => IsFrozen = true;

    public ImmutableArray<byte> GetReadOnlyData() => _data.ToImmutableArray();
}