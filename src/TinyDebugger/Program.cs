using System.Collections.Immutable;
using System.Diagnostics;
using Spectre.Console;
using TinyAssemblerLib;
using TinyCpuLib;
using TinyExt;
using static TinyAssemblerLib.TinyAsmTokenizer;
using OpCode = TinyCpuLib.OpCode;

var exe = new byte[]
{
    /*00:*/ 0x01, 0x05, 0xFF, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_1 FF
    /*06:*/ 0xB7, 0x05, 0x00, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_1 0
    /*0c:*/ 0xFF, // [HALT] HALT
};

//TODO: this should be a cpu interface, and the cpu should be running on a seprate process
var tinyCpu = new TinyCpu()
{
    TCpuExe = exe,
};


while (!tinyCpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
{
    AnsiConsole.Write(GetCpuInfo(tinyCpu));
    tinyCpu.Step();
}


return;

Table GetCpuInfo(TinyCpu cpu)
{
    var tbl = new Table();
    tbl.AddColumn("Registers");
    tbl.AddColumn("De-comp");

    tbl.AddRow(
        GetRegisterValueTable(cpu.Reg), GetDecomp(cpu.TCpuExe, cpu.Reg.INST_PTR)
    );
    return tbl;
}


Table GetDecomp(byte[] tcpuExe, int instPtr)
{
    var tokens = Decompile(tcpuExe);

    var tbl = new Table();
    tbl.AddColumns("PC", "ADDR", "OPCODE", "ARG 0", "ARG 1");
    foreach (var t in tokens)
    {
        var pc = (t.address == instPtr) switch
        {
            true => ">",
            false => "",
        };

        tbl.AddRow(
            pc,
            $"{t.address:X4}: ",
            $"{t.opCode}",
            t.sarg0,
            t.sarg1
        );
    }


    return tbl;
}


Table GetRegisterValueTable(CpuRegisters registers)
{
    var registerTable = new Table();

    registerTable.AddColumn("Register");
    registerTable.AddColumn("Value");

    for (var i = 0; i < (int)RegisterIndex.__REGISTER__COUNT__; i++)
    {
        var regStr = Enum.GetName((RegisterIndex)i) ?? throw new NullReferenceException();
        var val = registers.Data[i].ToString("X8");
        registerTable.AddRow(regStr, val);
    }

    return registerTable;
}

List<DecompToken> Decompile(byte[] texe)
{
    var que = new Queue<byte>(texe);
    var ret = new List<DecompToken>();
    var buf = new List<byte>();

    var cnt = 0; //current instruction bytes count
    var rib = -1; //Required instruction bytes

    int byteCount = 0;
    int instStartAddress = 0;
    var opCode = OpCode.NOOP;
    var argZeroType = Token.ArgumentType.NONE;
    var argOneType = Token.ArgumentType.NONE;


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

            if (argZeroType != Token.ArgumentType.NONE)
            {
                argZeroBytes = arr[1..(1 + arg0Size)];
                argOneBytes = arr[(1 + arg0Size)..(1 + 1 + arg1Size)];
            }

            if (argOneType != Token.ArgumentType.NONE)
            {
                argZeroValue = argZeroType.FromBytes(argZeroBytes);
                argOneValue = argOneType.FromBytes(argOneBytes);
            }

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

record DecompToken(OpCode opCode, Token.ArgumentType argZeroType, Token.ArgumentType argOneType,
    ImmutableArray<byte> data, byte[] arg0, byte[] arg1, string sarg0, string sarg1, int address);