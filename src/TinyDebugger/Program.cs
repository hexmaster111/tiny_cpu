using System.Collections.Immutable;
using System.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;
using TinyAssemblerLib;
using TinyCpuLib;
using TinyExt;
using static TinyAssemblerLib.TinyAsmTokenizer;
using OpCode = TinyCpuLib.OpCode;

var exe = new byte[]
{
    /*00:*/ 0xA6, // [CALL_D] LBL START
    /*01:*/ 0x0B, 0x04, // [INC] INC GP_I32_0
    /*03:*/ 0x0D, 0x04, 0x0E, 0x00, 0x00, 0x00, // [CMP_R_C] CMP GP_I32_0 E
    /*09:*/ 0xA9, 0x13, 0x00, 0x00, 0x00, // [JMP_C_GTR] JMP_GTR DONE
    /*0e:*/ 0xB4, 0x00, 0x00, 0x00, 0x00, // [JMP_C] JMP START
    /*13:*/ 0xA6, // [CALL_D] LBL DONE
    /*14:*/ 0x0B, 0x05, // [INC] INC GP_I32_1
    /*16:*/ 0xFF, // [HALT] HALT
};

//TODO: this should be a cpu interface, and the cpu should be running on a seprate process
var tinyCpu = new TinyCpu()
{
    TCpuExe = exe,
};

bool paused = true;

while (!tinyCpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
{
    AnsiConsole.Clear();
    AnsiConsole.Write(GetInfo(tinyCpu));
    if (paused) paused = WaitForKey(tinyCpu);
    else
    {
        tinyCpu.Step();
        Thread.Sleep(TimeSpan.FromSeconds(1f / tinyCpu.CycleTimeHz));
    }

    if (Console.KeyAvailable)
    {
        var key = Console.ReadKey();
        if (key.Key == ConsoleKey.Spacebar) paused = true;
    }
}
AnsiConsole.Clear();
AnsiConsole.Write(GetInfo(tinyCpu));
AnsiConsole.Write(new Markup("[red]CPU HALTED[/]"));

return;

bool WaitForKey(TinyCpu cpu)
{
    AnsiConsole.Write(new Markup("[yellow]S[/]TEP [yellow]C[/]ONTINUE [yellow]R[/]UN SPEED"));
    Console.WriteLine();
    var key = Console.ReadKey();

    if (key.Key == ConsoleKey.S) cpu.Step();
    else if (key.Key == ConsoleKey.R) cpu.CycleTimeHz = AnsiConsole.Ask("Speed (Hz)", cpu.CycleTimeHz);
    else if (key.Key == ConsoleKey.C) return false;

    return true;
}

Table GetInfo(TinyCpu cpu) => new Table().AddColumns("CPU Info", "Instruction decomp")
    .AddRow(GetCpuInfo(cpu), GetDecomp(tinyCpu.TCpuExe, tinyCpu.Reg.INST_PTR));

Table GetCpuInfo(TinyCpu cpu) => new Table().AddColumns("Registers", "FLAGS_0", "CPU INTERNALS")
    .AddRow(
        GetRegisterValueTable(cpu.Reg),
        GetFlagsRegisterTable<FLAGS_0_USAGE>(cpu.Reg.FLAGS_0),
        GetCpuInternalsTable(cpu)
    );

Table GetCpuInternalsTable(TinyCpu cpu) =>
    new Table().AddColumn("var").AddColumn("value")
        .AddRow("Cycles", cpu.Cycles.ToString())
        .AddRow("Set Speed (hz)", cpu.CycleTimeHz.ToString());

Table GetFlagsRegisterTable<TEnum>(int register) where TEnum : struct, Enum
{
    var tbl = new Table().AddColumn("name").AddColumn("value");

    foreach (var f in Enum.GetValues<TEnum>())
    {
        var regVal = register.ReadBit(f.GetHashCode());
        var valStr = regVal switch
        {
            true => "[green]T[/]",
            false => "[red]F[/]"
        };

        tbl.AddRow(f.ToString(), valStr);
    }

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
        var reg = (RegisterIndex)i;
        var regStr = Enum.GetName(reg)!;
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
                if (argOneType != Token.ArgumentType.NONE)
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

record DecompToken(OpCode opCode, Token.ArgumentType argZeroType, Token.ArgumentType argOneType,
    ImmutableArray<byte> data, byte[] arg0, byte[] arg1, string sarg0, string sarg1, int address);