using System.Collections.Immutable;
using System.Diagnostics;
using Decomp;
using Spectre.Console;
using TinyCpuLib;
using TinyExt;
using static TinyAssemblerLib.TinyAsmTokenizer;
using OpCode = TinyCpuLib.OpCode;

var exe = new byte[]
{
    /*00:*/ 0x01, 0x04, 0xFF, 0xFF, 0xFF, 0x00, // [SETREG_R_C] SETREG GP_I32_0 FFFFFF
    /*06:*/ 0xB7, 0x04, 0x00, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_0 0
    /*0c:*/ 0xB5, 0x05, 0x00, 0x00, 0x00, 0x00, // [MEM_READ_R_C] MEM_READ GP_I32_1 0
    /*12:*/ 0x0E, 0x04, 0x05, // [CMP_R_R] CMP GP_I32_0 GP_I32_1
    /*15:*/ 0xA8, 0x1F, 0x00, 0x00, 0x00, // [JMP_C_NEQ] JMP_NEQ FAIL
    /*1a:*/ 0xA7, 0x21, 0x00, 0x00, 0x00, // [JMP_C_EQ] JMP_EQ PASS
    /*1f:*/ 0xA6, // [CALL_D] LBL FAIL
    /*20:*/ 0xFF, // [HALT] HALT
    /*21:*/ 0xA6, // [CALL_D] LBL PASS
    /*22:*/ 0x0B, 0x06, // [INC] INC GP_I32_2
    /*24:*/ 0xFF, // [HALT] HALT
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

Table GetInfo(TinyCpu cpu) => new Table().AddColumns("CPU", "PROGRAM")
    .AddRow(GetCpuInfo(cpu), GetDecomp(tinyCpu.TCpuExe, tinyCpu.Reg.INST_PTR))
    .AddRow(new Markup("MEMORY"), new Markup("2"))
    .AddRow(GetMemoryDebugInfo(cpu), new Markup("2"));


Table GetMemoryDebugInfo(TinyCpu cpu)
{
    var tbl = new Table().AddColumns("ADDR", "VAL");
    var mem = cpu.Memory.Debugger_ReadAllMemoryAddresses();
    for (int i = 0; i < mem.Length; i++)
    {
        tbl.AddRow($"{i:X8}", $"{mem[i]}");
    }

    return tbl;
}

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

    for (var i = 0; i < (int)RegisterIndex.GP_I32_2 + 1; i++)
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

