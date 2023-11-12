using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection.Emit;
using TinyCpuLib;
using OpCode = TinyCpuLib.OpCode;

namespace TinyAssembler;

public class TinyAsmAssembler
{
    public byte[] Assembly { get; private set; } = Array.Empty<byte>();

    public ImmutableArray<TinyAsmTokenizer.Token> Tokens { get; private set; }
    public AsmToken[] AsmTokens { get; private set; } = Array.Empty<AsmToken>();

    public TinyAsmAssembler(ImmutableArray<TinyAsmTokenizer.Token> tokens) => Tokens = tokens;


    public byte[] Assemble()
    {
        AsmTokens = Tokens.Select(AsmToken.New).ToArray();

        //Some tokens need to know where tokens in fount of it will be, hence the two step process
        FinishTokens();

        List<byte> output = new();
        foreach (var at in AsmTokens)
        {
            if (!at.IsFrozen) Console.WriteLine($"[WARN] Token {at.Token} Not Final");
            output.AddRange(at.GetReadOnlyData());
        }

        return Assembly = output.ToArray();
    }

    private void FinishTokens()
    {
        foreach (var t in AsmTokens)
        {
            switch (t.Token.Type)
            {
                case TinyAsmTokenizer.Token.TokenType.ADD:
                case TinyAsmTokenizer.Token.TokenType.SUB:
                case TinyAsmTokenizer.Token.TokenType.DIV:
                case TinyAsmTokenizer.Token.TokenType.MUL:
                    throw new NotImplementedException("TODO: Assemble math instructions");


                case TinyAsmTokenizer.Token.TokenType.SETREG:
                    FixSetReg(t);
                    break;
                case TinyAsmTokenizer.Token.TokenType.CALL:
                    FixCall(t);
                    break;
                case TinyAsmTokenizer.Token.TokenType.HALT:
                case TinyAsmTokenizer.Token.TokenType.LBL:
                case TinyAsmTokenizer.Token.TokenType.NOOP:
                case TinyAsmTokenizer.Token.TokenType.RET:
                    t.Freeze();
                    break;
                case TinyAsmTokenizer.Token.TokenType.NONE:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            continue;
        }
    }

    private void FixCall(AsmToken asmToken)
    {
        if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            var data = asmToken.GetData();
            data[0] = (byte)OpCode.CALL_C;
            var dataBytes = BitConverter.GetBytes(GetIntConst(asmToken.Token.ArgumentZeroData));
            data[1] = dataBytes[0];
            data[2] = dataBytes[1];
            data[3] = dataBytes[2];
            data[4] = dataBytes[3];
        }
        else if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.REGISTER)
        {
            var data = asmToken.GetData();
            data[0] = (byte)OpCode.CALL_R;
            var regIndex = Enum.Parse<RegisterIndex>(asmToken.Token.ArgumentZeroData);
            data[1] = (byte)regIndex;
        }
        else if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.STR)
        {
            var targetInst = AsmTokens.Where(x => x.Token is
            {
                Type: TinyAsmTokenizer.Token.TokenType.LBL,
                ArgumentZeroType: TinyAsmTokenizer.Token.ArgumentType.STR
            }).ToImmutableArray();

            if (!targetInst.Any())
            {
                throw new InvalidParameterException($"Label \"{asmToken.Token.ArgumentZeroData}\" was not found");
            }

            if (targetInst.Length > 1)
            {
                throw new InvalidParameterException(
                    $"Label \"{asmToken.Token.ArgumentZeroData}\" was found more then once");
            }

            var lbl = targetInst.First();
            var addressDataBytes = GetIntConst(GetInstAddress(lbl));
            var data = asmToken.GetData();
            data[0] = (byte)OpCode.CALL_C;
            data[1] = addressDataBytes[0];
            data[2] = addressDataBytes[1];
            data[3] = addressDataBytes[2];
            data[4] = addressDataBytes[3];
        }

        asmToken.Freeze(); //Mark any call as Ready for output
    }

    private void FixSetReg(AsmToken asmToken)
    {
        if (asmToken.Token.ArgumentZeroType is not TinyAsmTokenizer.Token.ArgumentType.REGISTER)
        {
            throw new ArgumentException(
                $"First Arg of SetReg must be of type REGISTER, got {asmToken.Token.ArgumentZeroType}:" +
                $"\"{asmToken.Token.ArgumentZeroData}\"");
        }

        var argOneType = asmToken.Token.ArgumentOneType;
        var argOneData = asmToken.Token.ArgumentOneData;

        var argData = argOneType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => GetIntConst(GetIntConst(argOneData)),
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => new[] { GetRegisterByteConst(argOneData) },
            TinyAsmTokenizer.Token.ArgumentType.STR => throw new Exception("String arg not valid with set register"),
            TinyAsmTokenizer.Token.ArgumentType.NONE => throw new Exception("Expected a register or a constant"),
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = asmToken.GetData();
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        data[0] = argOneType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => (byte)OpCode.SETREG_R_C,
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => (byte)OpCode.SETREG_R_R,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (argOneType == TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            data[1] = argData[0];
            data[2] = argData[1];
            data[3] = argData[2];
            data[4] = argData[3];
        }
        else
        {
            data[1] = argData[0];
        }

        asmToken.Freeze();
    }

    private byte GetRegisterByteConst(string argData) => (byte)Enum.Parse<RegisterIndex>(argData);

    public int GetInstAddress(AsmToken lbl)
    {
        var addressAcm = 0;
        foreach (var t in AsmTokens)
        {
            if (t == lbl) return addressAcm;
            addressAcm += t.GetReadOnlyData().Length;
        }

        throw new InvalidParameterException($"Instruction Not found {lbl}-{lbl.Token}");
    }

    private static byte[] GetIntConst(int num) => BitConverter.GetBytes(num);
    private static int GetIntConst(string constTokenData) => Convert.ToInt32(constTokenData, 16);
}

public class AsmToken : IFreezable
{
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
        TinyAsmTokenizer.Token.TokenType.SETREG => new AsmToken(token, new byte[GetTokenSize(token)]),
        TinyAsmTokenizer.Token.TokenType.ADD => new AsmToken(token, new byte[GetTokenSize(token)]),
        TinyAsmTokenizer.Token.TokenType.SUB => new AsmToken(token, new byte[GetTokenSize(token)]),
        TinyAsmTokenizer.Token.TokenType.DIV => new AsmToken(token, new byte[GetTokenSize(token)]),
        TinyAsmTokenizer.Token.TokenType.MUL => new AsmToken(token, new byte[GetTokenSize(token)]),
        TinyAsmTokenizer.Token.TokenType.CALL => new AsmToken(token, new byte[GetTokenSize(token)]),

        TinyAsmTokenizer.Token.TokenType.NOOP => new AsmToken(token, new[] { (byte)OpCode.NOOP }),
        TinyAsmTokenizer.Token.TokenType.LBL => new AsmToken(token, new[] { (byte)OpCode.CALL_D }),
        TinyAsmTokenizer.Token.TokenType.HALT => new AsmToken(token, new[] { (byte)OpCode.HALT }),
        TinyAsmTokenizer.Token.TokenType.RET => new AsmToken(token, new[] { (byte)OpCode.RET }),
        TinyAsmTokenizer.Token.TokenType.NONE => throw new ArgumentOutOfRangeException(),
        _ => throw new ArgumentOutOfRangeException()
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
        TinyAsmTokenizer.Token parentToken) =>
        argType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => 4,
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => 1,
            TinyAsmTokenizer.Token.ArgumentType.STR => parentToken.Type switch
            {
                TinyAsmTokenizer.Token.TokenType.NOOP => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.SETREG => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.ADD => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.SUB => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.DIV => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.MUL => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.LBL => 0, //label just turns into a CALLDST and thats just the opcode
                TinyAsmTokenizer.Token.TokenType.CALL => 4, //INT address
                TinyAsmTokenizer.Token.TokenType.HALT => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.RET => throw new InvalidParameterException(),
                TinyAsmTokenizer.Token.TokenType.NONE => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            },
            TinyAsmTokenizer.Token.ArgumentType.NONE => 0,
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

public class InvalidParameterException : Exception
{
    public string Detail { get; } = "";

    public InvalidParameterException()
    {
    }

    public InvalidParameterException(string s) : base(s)
    {
    }

    public InvalidParameterException(string s, string detail) : base(s)
    {
        Detail = detail;
    }
}