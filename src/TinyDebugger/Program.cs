using Spectre.Console;
using TinyCpuLib;

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
    return new Table();
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