namespace TinyCpuLib;

public readonly struct VirtualMemory : IMemory
{
    public readonly int[] GeneralUse;

    public VirtualMemory() => GeneralUse = new int[16];
    public int MemorySize { get; } = 16;

    public int Read(int address) => GeneralUse[address];
    public void Write(int address, int value) => GeneralUse[address] = value;

    public int this[int address]
    {
        get => Read(address);
        set => Write(address, value);
    }

    public int[] Debugger_ReadAllMemoryAddresses() => GeneralUse;
}