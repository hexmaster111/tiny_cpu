using System.Collections.Immutable;
using TinyCpuLib;
using OpCode = TinyCpuLib.OpCode;

namespace TinyAssemblerLib;

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
                case TinyAsmTokenizer.Token.TokenType.JMP_EQ:
                    FixJmp(t, OpCode.JMP_INTC_EQ, OpCode.JMP_INTR_EQ);
                    break;
                case TinyAsmTokenizer.Token.TokenType.JMP_NEQ:
                    FixJmp(t, OpCode.JMP_INTC_NEQ, OpCode.JMP_INTR_NEQ);
                    break;
                case TinyAsmTokenizer.Token.TokenType.JMP_GTR:
                    FixJmp(t, OpCode.JMP_INTC_GTR, OpCode.JMP_INTR_GTR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.JMP_GEQ:
                    FixJmp(t, OpCode.JMP_INTC_GEQ, OpCode.JMP_INTR_GEQ);
                    break;
                case TinyAsmTokenizer.Token.TokenType.JMP_LES:
                    FixJmp(t, OpCode.JMP_INTC_LES, OpCode.JMP_INTR_LES);
                    break;
                case TinyAsmTokenizer.Token.TokenType.JMP_LEQ:
                    FixJmp(t, OpCode.JMP_INTC_LEQ, OpCode.JMP_INTR_LEQ);
                    break;
                case TinyAsmTokenizer.Token.TokenType.JMP:
                    FixJmp(t, OpCode.JMP_INTC, OpCode.JMP_INTR);
                    break;

                case TinyAsmTokenizer.Token.TokenType.ADD:
                    FixMathInst(t, OpCode.ADD_INTR_INTC, OpCode.ADD_INTR_INTR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.SUB:
                    FixMathInst(t, OpCode.SUB_INTR_INTC, OpCode.SUB_INTR_INTR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.DIV:
                    FixMathInst(t, OpCode.DIV_INTR_INTC, OpCode.DIV_INTR_INTR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.MUL:
                    FixMathInst(t, OpCode.MUL_INTR_INTC, OpCode.MUL_INTR_INTR);
                    break;

                case TinyAsmTokenizer.Token.TokenType.INC:
                    FixSingleRegOnly(t, OpCode.INC_INTR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.DEC:
                    FixSingleRegOnly(t, OpCode.DEC_INTR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.POP:
                    FixSingleRegOnly(t, OpCode.POP_INTR);
                    break;

                case TinyAsmTokenizer.Token.TokenType.PUSH:
                    FixPushInst(t);
                    break;

                case TinyAsmTokenizer.Token.TokenType.SETREG:
                    Fix_RR_RC(t, OpCode.SETREG_INTR_INTR, OpCode.SETREG_INTR_INTC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.CMP:
                    Fix_RR_RC(t, OpCode.CMP_INTR_INTR, OpCode.CMP_INTR_INTC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.CALL:
                    FixCall(t, OpCode.CALL_INTC, OpCode.CALL_INTR); //CALL R?!
                    break;
                case TinyAsmTokenizer.Token.TokenType.HALT:
                case TinyAsmTokenizer.Token.TokenType.LBL:
                case TinyAsmTokenizer.Token.TokenType.NOOP:
                case TinyAsmTokenizer.Token.TokenType.RET:
                    t.Freeze();
                    break;
                case TinyAsmTokenizer.Token.TokenType.MEM_READ:
                    Fix_RR_RC(t, OpCode.MEM_READ_INTR_INTR, OpCode.MEM_READ_INTR_INTC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.MEM_WRITE:
                    Fix_RR_RC(t, OpCode.MEM_WRITE_INTR_INTR, OpCode.MEM_WRITE_INTR_INTC);
                    break;

                case TinyAsmTokenizer.Token.TokenType.NONE:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }


    private void FixJmp(AsmToken token, OpCode jmpCOp, OpCode jmpROp)
    {
        var token0Type = token.Token.ArgumentZeroType;
        var token1Type = token.Token.ArgumentOneType;
        if (token1Type != TinyAsmTokenizer.Token.ArgumentType.NONE)
            throw new Exception("Jmp* only takes 1 arg");

        FixCall(token, jmpCOp, jmpROp);
    }

    private void FixPushInst(AsmToken token)
    {
        var token0Type = token.Token.ArgumentZeroType;
        var token1Type = token.Token.ArgumentOneType;

        if (token0Type != TinyAsmTokenizer.Token.ArgumentType.REGISTER &&
            token0Type != TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            throw new Exception($"Arg 0 must be of type reg or const, got {token0Type}");
        }

        var opcode = token0Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => (byte)OpCode.PUSH_INTC,
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => (byte)OpCode.PUSH_INTR,
            _ => throw new ArgumentOutOfRangeException()
        };

        var argZeroBytes = token0Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => GetIntConst(GetIntConst(token.Token.ArgumentZeroData)),
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => new[]
                { GetRegisterByteConst(token.Token.ArgumentZeroData) },
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = token.GetData();
        data[0] = opcode;
        data[1] = argZeroBytes[0];
        if (token0Type == TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            data[2] = argZeroBytes[1];
            data[3] = argZeroBytes[2];
            data[4] = argZeroBytes[3];
        }

        token.Freeze();
    }

    private void FixSingleRegOnly(AsmToken token, OpCode code)
    {
        var token0Type = token.Token.ArgumentZeroType;
        var token1Type = token.Token.ArgumentOneType;

        if (token0Type != TinyAsmTokenizer.Token.ArgumentType.REGISTER)
            throw new Exception($"Arg 0 of {token.Token.Type} must be a register");

        if (token1Type != TinyAsmTokenizer.Token.ArgumentType.NONE)
            throw new Exception($"{token.Token.Type} Unexpected arg");


        var destRegAddr = GetRegisterByteConst(token.Token.ArgumentZeroData);
        var data = token.GetData();
        data[0] = (byte)code;
        data[1] = destRegAddr;
        token.Freeze();
    }

    private void FixMathInst(AsmToken token, OpCode constVerOpCode, OpCode registerVerOpCode)
    {
        var token0Type = token.Token.ArgumentZeroType;
        var token1Type = token.Token.ArgumentOneType;

        if (token0Type is not TinyAsmTokenizer.Token.ArgumentType.REGISTER)
            throw new Exception($"Arg 0 must be of type register, got {token0Type}");

        if (token1Type != TinyAsmTokenizer.Token.ArgumentType.REGISTER &&
            token1Type != TinyAsmTokenizer.Token.ArgumentType.CONST)
            throw new Exception($"Arg 1 must be of type register or constant, got {token1Type}");

        var dstRegister = GetRegisterByteConst(token.Token.ArgumentZeroData);
        var token1Data = token1Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => GetIntConst(GetIntConst(token.Token.ArgumentOneData)),
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => new[] { GetRegisterByteConst(token.Token.ArgumentOneData) },
            _ => throw new ArgumentOutOfRangeException()
        };

        var opCode = token1Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => (byte)constVerOpCode,
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => (byte)registerVerOpCode,
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = token.GetData();
        data[0] = opCode;
        data[1] = dstRegister;
        data[2] = token1Data[0];
        if (token1Type == TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            data[3] = token1Data[1];
            data[4] = token1Data[2];
            data[5] = token1Data[3];
        }

        token.Freeze();
    }

    private void FixCall(AsmToken asmToken, OpCode constOpCode, OpCode registerOpCode)
    {
        if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            var data = asmToken.GetData();
            data[0] = (byte)constOpCode;
            var dataBytes = BitConverter.GetBytes(GetIntConst(asmToken.Token.ArgumentZeroData));
            data[1] = dataBytes[0];
            data[2] = dataBytes[1];
            data[3] = dataBytes[2];
            data[4] = dataBytes[3];
        }
        else if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.REGISTER)
        {
            var data = asmToken.GetData();
            data[0] = (byte)registerOpCode;
            var regIndex = Enum.Parse<IntRegisterIndex>(asmToken.Token.ArgumentZeroData);
            data[1] = (byte)regIndex;
        }
        else if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.STR)
        {
            var targetInst = AsmTokens.Where(x => x.Token is
                {
                    Type: TinyAsmTokenizer.Token.TokenType.LBL,
                    ArgumentZeroType: TinyAsmTokenizer.Token.ArgumentType.STR
                }).Where(x => x.Token.ArgumentZeroData == asmToken.Token.ArgumentZeroData)
                .ToImmutableArray();

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
            data[0] = (byte)constOpCode;
            data[1] = addressDataBytes[0];
            data[2] = addressDataBytes[1];
            data[3] = addressDataBytes[2];
            data[4] = addressDataBytes[3];
        }

        asmToken.Freeze(); //Mark any call as Ready for output
    }

    private void Fix_RR_RC(AsmToken asmToken, OpCode rr, OpCode rc)
    {
        if (asmToken.Token.ArgumentZeroType is not TinyAsmTokenizer.Token.ArgumentType.REGISTER)
        {
            throw new ArgumentException(
                $"First Arg of {asmToken.Token.Type} must be of type REGISTER, got {asmToken.Token.ArgumentZeroType}:" +
                $"\"{asmToken.Token.ArgumentZeroData}\"");
        }

        var argOneType = asmToken.Token.ArgumentOneType;
        var argOneData = asmToken.Token.ArgumentOneData;

        var arg1Data = argOneType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => GetIntConst(GetIntConst(argOneData)),
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => new[] { GetRegisterByteConst(argOneData) },
            TinyAsmTokenizer.Token.ArgumentType.STR => throw new Exception(
                $"String arg not valid with set register for {asmToken}"),
            TinyAsmTokenizer.Token.ArgumentType.NONE => throw new Exception(
                $"Expected a register or a constant for {asmToken}"),
            _ => throw new ArgumentOutOfRangeException()
        };
        var argZeroData = GetRegisterByteConst(asmToken.Token.ArgumentZeroData);


        var data = asmToken.GetData();
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        data[0] = argOneType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => (byte)rc,
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => (byte)rr,
            _ => throw new ArgumentOutOfRangeException()
        };

        data[1] = argZeroData;
        data[2] = arg1Data[0];

        if (argOneType == TinyAsmTokenizer.Token.ArgumentType.CONST)
        {
            data[3] = arg1Data[1];
            data[4] = arg1Data[2];
            data[5] = arg1Data[3];
        }

        asmToken.Freeze();
    }

    private byte GetRegisterByteConst(string argData) => (byte)Enum.Parse<IntRegisterIndex>(argData);

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
        TinyAsmTokenizer.Token parentToken) =>
        argType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.CONST => 4,
            TinyAsmTokenizer.Token.ArgumentType.REGISTER => 1,
            TinyAsmTokenizer.Token.ArgumentType.STR => parentToken.Type switch
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