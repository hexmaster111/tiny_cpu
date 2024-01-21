using System.Collections.Immutable;
using System.Diagnostics;
using TinyAssemblerLib;
using TinyCpuLib;
using TinyExt;

namespace Decomp;

public record DecompToken(
    OpCode opCode,
    TinyAsmTokenizer.Token.ArgumentType argZeroType,
    TinyAsmTokenizer.Token.ArgumentType argOneType,
    ImmutableArray<byte> data,
    byte[] arg0,
    byte[] arg1,
    string sarg0,
    string sarg1,
    int address);

public class TinyAsmDecompiler
{
    public List<DecompToken> Decompile(byte[] texe)
    {
        var que = new Queue<byte>(texe);
        var ret = new List<DecompToken>();
        var buf = new List<byte>();

        var cnt = 0; //current instruction bytes count
        var rib = -1; //Required instruction bytes

        int byteCount = 0;
        int instStartAddress = 0;
        var opCode = OpCode.NOOP;
        var argZeroType = TinyAsmTokenizer.Token.ArgumentType.NONE;
        var argOneType = TinyAsmTokenizer.Token.ArgumentType.NONE;
        int arg0Size = 0;
        int arg1Size = 0;

        while (que.TryDequeue(out var b))
        {
            //We are getting the next opcode we are hunting
            if (rib == -1)
            {
                cnt = 0;
                var op = (OpCode)b;

                switch (op)
                {
                    case OpCode.SETREG_STRR_STRC:
                    case OpCode.CCAT_STRR_STRC:
                    case OpCode.CMP_STRR_STRC:
                    case OpCode.PUSH_STRC:
                    {
                        opCode = op;
                        instStartAddress = byteCount;
                        argZeroType = op.ArgOneType();
                        argOneType = op.ArgTwoType();

                        if (argZeroType == TinyAsmTokenizer.Token.ArgumentType.StrLiteral)
                        {
                            var a0StrLen = GetStrLen(texe, instStartAddress + 1);
                            arg0Size = a0StrLen;
                            rib = 2 + a0StrLen; // 1 byte Opcode a0StrLen
                            break;
                        }
                        else
                        {
                            arg0Size = argZeroType.ByteCount();
                        }

                        var strLen = GetStrLen(texe, instStartAddress + 2);
                        arg1Size = strLen;
                        // +1 skip arg0 ( 1 byte) +1 opcode
                        rib = 3 + strLen; // 1 byte Opcode + 1 byte for arg0 + strLen  + 1 byte for null terminator
                        break;
                    }
                    default:
                        rib = op.GetInstructionByteCount();
                        opCode = op;
                        argZeroType = op.ArgOneType();
                        argOneType = op.ArgTwoType();
                        arg0Size = argZeroType.ByteCount();
                        arg1Size = argOneType.ByteCount();
                        instStartAddress = byteCount;
                        break;
                }
            }


            buf.Add(b);
            cnt++;


            if (rib <= cnt && rib != -1)
            {
                rib = -1;

                var arr = buf.ToArray();
                byte[] argZeroBytes = Array.Empty<byte>();
                byte[] argOneBytes = Array.Empty<byte>();
                var argZeroValue = "";
                var argOneValue = "";

                if (argZeroType != TinyAsmTokenizer.Token.ArgumentType.NONE)
                {
                    argZeroBytes = arr[1..(1 + arg0Size)];
                    if (argOneType != TinyAsmTokenizer.Token.ArgumentType.NONE)
                        argOneBytes = arr[(1 + arg0Size)..(1 + 1 + arg1Size)];
                }

                argZeroValue = argZeroType.FromBytes(argZeroBytes);
                argOneValue = argOneType.FromBytes(argOneBytes);


                ret.Add(new DecompToken(
                    /*OpCode*/ opCode,
                    /*ArgumentType*/ argZeroType,
                    /*ArgumentType*/ argOneType,
                    /*ImmutableArray*/ buf.ToImmutableArray(),
                    /*byte[]*/ argZeroBytes,
                    /*byte[]*/ argOneBytes,
                    /*string*/ argZeroValue,
                    /*string*/ argOneValue,
                    /*int*/ instStartAddress
                ));
                buf = new List<byte>();
            }

            byteCount++;
        }

        Debug.Assert(!que.Any(), "Finished early");
        return ret;
    }

    private int GetStrLen(byte[] texe, int instStartAddress)
    {
        var strLen = 0;
        while (texe[instStartAddress + strLen] != 0x00) strLen++;
        return strLen;
    }
}

public static class Ext
{
    public static int ByteCount(this TinyAsmTokenizer.Token.ArgumentType t) => t switch
    {
        TinyAsmTokenizer.Token.ArgumentType.ConstInt => sizeof(int),
        TinyAsmTokenizer.Token.ArgumentType.IntRegister => sizeof(byte),
        TinyAsmTokenizer.Token.ArgumentType.NONE => 0,
        TinyAsmTokenizer.Token.ArgumentType.FuncName => throw new InvalidOperationException("func name _ lbl"),
        TinyAsmTokenizer.Token.ArgumentType.StrLiteral => throw new InvalidOperationException("str"),
        TinyAsmTokenizer.Token.ArgumentType.StrRegister => sizeof(byte),
        _ => throw new ArgumentOutOfRangeException(nameof(t), t, null)
    };


    public static TinyAsmTokenizer.Token.ArgumentType ArgTwoType(this OpCode oc) => oc switch
    {
        OpCode.NOOP => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.INC_INTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.DEC_INTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.CALL_INTC => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.CALL_INTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.RET => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.CALLD => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC_EQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC_NEQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC_GTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC_GEQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC_LES => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC_LEQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR_EQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR_NEQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR_GTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR_GEQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR_LES => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR_LEQ => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.JMP_INTC => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.PUSH_INTC => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.PUSH_INTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.POP_INTR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.PUSH_STRR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.PUSH_STRC => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.POP_STRR => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.HALT => TinyAsmTokenizer.Token.ArgumentType.NONE,

        OpCode.SETREG_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.ADD_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.MUL_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.DIV_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.SUB_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.MEM_READ_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.MEM_WRITE_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,

        OpCode.SETREG_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.ADD_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MUL_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.SUB_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.DIV_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.CMP_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.CMP_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_READ_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_WRITE_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_READ_STRR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_WRITE_STRR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,

        OpCode.MEM_READ_STRR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.MEM_WRITE_STRR_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,

        OpCode.SETREG_STRR_STRC => TinyAsmTokenizer.Token.ArgumentType.StrLiteral,
        OpCode.CCAT_STRR_STRC => TinyAsmTokenizer.Token.ArgumentType.StrLiteral,
        OpCode.CMP_STRR_STRC => TinyAsmTokenizer.Token.ArgumentType.StrLiteral,

        OpCode.SETREG_STRR_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.CCAT_STRR_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.CMP_STRR_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,

        _ => throw new ArgumentOutOfRangeException(nameof(oc), oc, null)
    };

    public static TinyAsmTokenizer.Token.ArgumentType ArgOneType(this OpCode oc) => oc switch
    {
        OpCode.NOOP => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.RET => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.CALLD => TinyAsmTokenizer.Token.ArgumentType.NONE,
        OpCode.HALT => TinyAsmTokenizer.Token.ArgumentType.NONE,

        OpCode.MEM_READ_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_READ_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_WRITE_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MEM_WRITE_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.SETREG_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.SETREG_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.ADD_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.ADD_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MUL_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.MUL_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.SUB_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.SUB_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.DIV_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.DIV_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.INC_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.DEC_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.CMP_INTR_INTC => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.CMP_INTR_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.PUSH_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.POP_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.CALL_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR_EQ => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR_NEQ => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR_GTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR_GEQ => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR_LES => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR_LEQ => TinyAsmTokenizer.Token.ArgumentType.IntRegister,
        OpCode.JMP_INTR => TinyAsmTokenizer.Token.ArgumentType.IntRegister,

        OpCode.PUSH_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.CALL_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC_EQ => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC_NEQ => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC_GTR => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC_GEQ => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC_LES => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC_LEQ => TinyAsmTokenizer.Token.ArgumentType.ConstInt,
        OpCode.JMP_INTC => TinyAsmTokenizer.Token.ArgumentType.ConstInt,

        OpCode.SETREG_STRR_STRC => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.SETREG_STRR_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.CCAT_STRR_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.CCAT_STRR_STRC => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.CMP_STRR_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.CMP_STRR_STRC => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.MEM_READ_STRR_INTC => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.MEM_READ_STRR_INTR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.MEM_WRITE_STRR_INTC => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.MEM_WRITE_STRR_INTR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.PUSH_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,
        OpCode.POP_STRR => TinyAsmTokenizer.Token.ArgumentType.StrRegister,

        OpCode.PUSH_STRC => TinyAsmTokenizer.Token.ArgumentType.StrLiteral,

        _ => throw new ArgumentOutOfRangeException(nameof(oc), oc, null)
    };
}