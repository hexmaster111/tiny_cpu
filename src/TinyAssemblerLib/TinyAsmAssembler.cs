using System.Collections.Immutable;
using System.Text;
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
                    FixSingleRegOnly(t, OpCode.INC_INTR, OpCode.NOOP);
                    break;
                case TinyAsmTokenizer.Token.TokenType.DEC:
                    FixSingleRegOnly(t, OpCode.DEC_INTR, OpCode.NOOP);
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
                case TinyAsmTokenizer.Token.TokenType.SCCAT:
                    FixCCat(t);
                    break;
                case TinyAsmTokenizer.Token.TokenType.POP:
                    FixSingleRegOnly(t, OpCode.POP_INTR, OpCode.POP_STRR);
                    break;
                case TinyAsmTokenizer.Token.TokenType.PUSH:
                    FixPushInst(t);
                    break;
                case TinyAsmTokenizer.Token.TokenType.SETREG:
                    Fix_intRR_intRC(t,
                        OpCode.SETREG_INTR_INTR, OpCode.SETREG_INTR_INTC,
                        OpCode.SETREG_STRR_STRR, OpCode.SETREG_STRR_STRC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.CMP:
                    Fix_intRR_intRC(t,
                        OpCode.CMP_INTR_INTR, OpCode.CMP_INTR_INTC,
                        OpCode.CMP_STRR_STRR, OpCode.CMP_STRR_STRC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.MEM_READ:
                    Fix_intRR_intRC(t,
                        OpCode.MEM_READ_INTR_INTR, OpCode.MEM_READ_INTR_INTC,
                        OpCode.MEM_READ_STRR_INTR, OpCode.MEM_READ_STRR_INTC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.MEM_WRITE:
                    Fix_intRR_intRC(t,
                        OpCode.MEM_WRITE_INTR_INTR, OpCode.MEM_WRITE_INTR_INTC,
                        OpCode.MEM_WRITE_STRR_INTR, OpCode.MEM_WRITE_STRR_INTC);
                    break;
                case TinyAsmTokenizer.Token.TokenType.NONE:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }



    private void FixCCat(AsmToken asmToken)
    {
        var token0Type = asmToken.Token.ArgumentZeroType;
        var token1Type = asmToken.Token.ArgumentOneType;

        if (token0Type != TinyAsmTokenizer.Token.ArgumentType.StrRegister)
            throw new Exception($"Arg 0 must be of type StrRegister, got {token0Type}");

        if (token1Type != TinyAsmTokenizer.Token.ArgumentType.StrRegister &&
            token1Type != TinyAsmTokenizer.Token.ArgumentType.StrLiteral)
            throw new Exception($"Arg 1 must be of type StrRegister or StrLiteral, got {token1Type}");


        var argZeroData = GetStrRegisterByteValue(asmToken.Token.ArgumentZeroData);
        var argOneData = token1Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => new byte[]
                { GetStrRegisterByteValue(asmToken.Token.ArgumentOneData) },
            TinyAsmTokenizer.Token.ArgumentType.StrLiteral => Encoding.ASCII.GetBytes(asmToken.Token.ArgumentOneData),
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = asmToken.GetData();
        data[0] = token1Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => (byte)OpCode.CCAT_STRR_STRR,
            TinyAsmTokenizer.Token.ArgumentType.StrLiteral => (byte)OpCode.CCAT_STRR_STRC,
            _ => throw new ArgumentOutOfRangeException()
        };

        data[1] = argZeroData;
        data[2] = argOneData[0];
        if (token1Type == TinyAsmTokenizer.Token.ArgumentType.StrLiteral)
        {
            Array.Copy(argOneData, 0, data, 2, argOneData.Length);
            data[^1] = 0;
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

        if (token0Type != TinyAsmTokenizer.Token.ArgumentType.IntRegister &&
            token0Type != TinyAsmTokenizer.Token.ArgumentType.ConstInt &&
            token0Type != TinyAsmTokenizer.Token.ArgumentType.StrRegister &&
            token0Type != TinyAsmTokenizer.Token.ArgumentType.StrLiteral)
        {
            throw new Exception($"Arg 0 must be of type reg or const, got {token0Type}");
        }

        var opcode = token0Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.ConstInt => (byte)OpCode.PUSH_INTC,
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => (byte)OpCode.PUSH_INTR,
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => (byte)OpCode.PUSH_STRR,
            TinyAsmTokenizer.Token.ArgumentType.StrLiteral => (byte)OpCode.PUSH_STRC,
            _ => throw new ArgumentOutOfRangeException()
        };

        var argZeroBytes = token0Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.ConstInt => GetIntConst(GetIntConst(token.Token.ArgumentZeroData)),
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => new[]
                { GetIntRegisterByteValue(token.Token.ArgumentZeroData) },
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => new[]
                { GetStrRegisterByteValue(token.Token.ArgumentZeroData) },
            TinyAsmTokenizer.Token.ArgumentType.StrLiteral => Encoding.ASCII.GetBytes(token.Token.ArgumentZeroData),
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = token.GetData();
        data[0] = opcode;
        data[1] = argZeroBytes[0];
        if (token0Type == TinyAsmTokenizer.Token.ArgumentType.ConstInt)
        {
            data[2] = argZeroBytes[1];
            data[3] = argZeroBytes[2];
            data[4] = argZeroBytes[3];
        }

        if (token0Type == TinyAsmTokenizer.Token.ArgumentType.StrLiteral)
        {
            argZeroBytes[^1] = 0;
            Array.Copy(argZeroBytes, 0, data, 2, argZeroBytes.Length);
        }

        token.Freeze();
    }

    private void FixSingleRegOnly(AsmToken token, OpCode code_i, OpCode code_s)
    {
        var token0Type = token.Token.ArgumentZeroType;
        var token1Type = token.Token.ArgumentOneType;

        if (!(token0Type == TinyAsmTokenizer.Token.ArgumentType.IntRegister ||
              token0Type == TinyAsmTokenizer.Token.ArgumentType.StrRegister))
            throw new Exception($"Arg 0 of {token.Token.Type} must be a register");

        if (token1Type != TinyAsmTokenizer.Token.ArgumentType.NONE)
            throw new Exception($"{token.Token.Type} Unexpected arg");


        var destRegAddr = token0Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => GetIntRegisterByteValue(token.Token.ArgumentZeroData),
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => GetStrRegisterByteValue(token.Token.ArgumentZeroData),
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = token.GetData();
        data[0] = token0Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => (byte)code_i,
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => (byte)code_s,
            _ => throw new ArgumentOutOfRangeException()
        };
        data[1] = destRegAddr;
        token.Freeze();
    }

    private void FixMathInst(AsmToken token, OpCode constVerOpCode, OpCode registerVerOpCode)
    {
        var token0Type = token.Token.ArgumentZeroType;
        var token1Type = token.Token.ArgumentOneType;

        if (token0Type is not TinyAsmTokenizer.Token.ArgumentType.IntRegister)
            throw new Exception($"Arg 0 must be of type register, got {token0Type}");

        if (token1Type != TinyAsmTokenizer.Token.ArgumentType.IntRegister &&
            token1Type != TinyAsmTokenizer.Token.ArgumentType.ConstInt)
            throw new Exception($"Arg 1 must be of type register or constant, got {token1Type}");

        var dstRegister = GetIntRegisterByteValue(token.Token.ArgumentZeroData);
        var token1Data = token1Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.ConstInt => GetIntConst(GetIntConst(token.Token.ArgumentOneData)),
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => new[]
                { GetIntRegisterByteValue(token.Token.ArgumentOneData) },
            _ => throw new ArgumentOutOfRangeException()
        };

        var opCode = token1Type switch
        {
            TinyAsmTokenizer.Token.ArgumentType.ConstInt => (byte)constVerOpCode,
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => (byte)registerVerOpCode,
            _ => throw new ArgumentOutOfRangeException()
        };

        var data = token.GetData();
        data[0] = opCode;
        data[1] = dstRegister;
        data[2] = token1Data[0];
        if (token1Type == TinyAsmTokenizer.Token.ArgumentType.ConstInt)
        {
            data[3] = token1Data[1];
            data[4] = token1Data[2];
            data[5] = token1Data[3];
        }

        token.Freeze();
    }

    private void FixCall(AsmToken asmToken, OpCode constOpCode, OpCode registerOpCode)
    {
        if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.ConstInt)
        {
            var data = asmToken.GetData();
            data[0] = (byte)constOpCode;
            var dataBytes = BitConverter.GetBytes(GetIntConst(asmToken.Token.ArgumentZeroData));
            data[1] = dataBytes[0];
            data[2] = dataBytes[1];
            data[3] = dataBytes[2];
            data[4] = dataBytes[3];
        }
        else if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.IntRegister)
        {
            var data = asmToken.GetData();
            data[0] = (byte)registerOpCode;
            var regIndex = Enum.Parse<IntRegisterIndex>(asmToken.Token.ArgumentZeroData);
            data[1] = (byte)regIndex;
        }
        else if (asmToken.Token.ArgumentZeroType == TinyAsmTokenizer.Token.ArgumentType.FuncName)
        {
            var targetInst = AsmTokens.Where(x => x.Token is
                {
                    Type: TinyAsmTokenizer.Token.TokenType.LBL,
                    ArgumentZeroType: TinyAsmTokenizer.Token.ArgumentType.FuncName
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

    private void Fix_intRR_intRC(AsmToken asmToken, OpCode int_r_r, OpCode int_r_c, OpCode str_r_r, OpCode str_r_c)
    {
        if (asmToken.Token.ArgumentZeroType != TinyAsmTokenizer.Token.ArgumentType.IntRegister &&
            asmToken.Token.ArgumentZeroType != TinyAsmTokenizer.Token.ArgumentType.StrRegister)
        {
            throw new ArgumentException(
                $"First Arg of {asmToken.Token.Type} must be of type IntRegister or StrRegister," +
                $" got {asmToken.Token.ArgumentZeroType}:" +
                $"\"{asmToken.Token.ArgumentZeroData}\"");
        }


        var arg1DataStr = asmToken.Token.ArgumentOneData;
        var arg1Data = asmToken.Token.ArgumentOneType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.ConstInt => GetIntConst(GetIntConst(arg1DataStr)),
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => new[] { GetIntRegisterByteValue(arg1DataStr) },
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => new[] { GetStrRegisterByteValue(arg1DataStr) },
            TinyAsmTokenizer.Token.ArgumentType.StrLiteral => new byte[Encoding.ASCII.GetBytes(arg1DataStr).Length + 1],
            _ => throw new ArgumentOutOfRangeException()
        };


        var argZeroData = asmToken.Token.ArgumentZeroType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => GetIntRegisterByteValue(asmToken.Token.ArgumentZeroData),
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => GetStrRegisterByteValue(asmToken.Token.ArgumentZeroData),
            _ => throw new ArgumentOutOfRangeException()
        };


        var data = asmToken.GetData();
        //Setting the opcode for the instruction based on the data type of the first argument
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        data[0] = asmToken.Token.ArgumentOneType switch
        {
            TinyAsmTokenizer.Token.ArgumentType.ConstInt => (byte)int_r_c,
            TinyAsmTokenizer.Token.ArgumentType.IntRegister => (byte)int_r_r,
            TinyAsmTokenizer.Token.ArgumentType.StrLiteral => (byte)str_r_c,
            TinyAsmTokenizer.Token.ArgumentType.StrRegister => (byte)str_r_r,
            _ => throw new ArgumentOutOfRangeException()
        };

        data[1] = argZeroData;
        data[2] = arg1Data[0];

        if (asmToken.Token.ArgumentOneType == TinyAsmTokenizer.Token.ArgumentType.ConstInt)
        {
            data[3] = arg1Data[1];
            data[4] = arg1Data[2];
            data[5] = arg1Data[3];
        }

        if (asmToken.Token.ArgumentOneType == TinyAsmTokenizer.Token.ArgumentType.StrLiteral)
        {
            var dat = Encoding.ASCII.GetBytes(arg1DataStr);
            Array.Copy(dat, 0, data, 2, dat.Length);
            //Null terminate the string
            data[^1] = 0;
        }

        Console.WriteLine($"Token line : {asmToken.Token.ToString()}");
        Console.WriteLine($"{arg1DataStr} -> {string.Join(", ", arg1Data.Select(x => $"0x{x:x2}"))}");
        Console.WriteLine($"{asmToken.Token.ArgumentZeroData} -> {argZeroData:x2}");
        asmToken.Freeze();
    }

    private byte GetIntRegisterByteValue(string argData) => (byte)Enum.Parse<IntRegisterIndex>(argData);
    private byte GetStrRegisterByteValue(string argData) => (byte)Enum.Parse<StrRegisterIndex>(argData);

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