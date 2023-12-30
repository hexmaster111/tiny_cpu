namespace TinyCpuLib;

public readonly struct VirtualMemory : IMemory
{
    public readonly int[] IntMem;
    public readonly string[] StrMem;
    public int MemorySize { get; } = 16;

    public VirtualMemory()
    {
        IntMem = new int[16];
        StrMem = new string[16];
    }

    public string ReadStr(int memAddress) => StrMem[memAddress];
    public void WriteStr(int memAddress, string value) => StrMem[memAddress] = value;

    public int ReadInt(int address) => IntMem[address];
    public void WriteInt(int address, int value) => IntMem[address] = value;

    public int this[int address]
    {
        get => ReadInt(address);
        set => WriteInt(address, value);
    }

    public int[] Debugger_ReadAllIntMemoryAddresses() => IntMem;
}