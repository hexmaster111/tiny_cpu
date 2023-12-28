namespace TinyCpuLib;

public interface IMemory
{
    public int Read(int address);
    public void Write(int address, int value);
    public int this[int address] { get; set; }
    public int[] Debugger_ReadAllMemoryAddresses();
    public int MemorySize { get; }
}