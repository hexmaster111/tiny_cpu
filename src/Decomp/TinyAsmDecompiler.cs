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


        while (que.TryDequeue(out var b))
        {
            //We are getting the next opcode we are hunting
            if (rib == -1)
            {
                cnt = 0;
                var op = ((OpCode)b);
                rib = op.GetInstructionByteCount();
                opCode = op;
                argZeroType = op.ArgOneType();
                argOneType = op.ArgTwoType();
                instStartAddress = byteCount;
            }

            buf.Add(b);
            cnt++;


            if (rib <= cnt)
            {
                rib = -1;

                int arg0Size = argZeroType.ByteCount();
                int arg1Size = argOneType.ByteCount();
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
}