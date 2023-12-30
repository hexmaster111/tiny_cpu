namespace TinyCpuLib;

public interface IMemory
{
    public int ReadInt(int address);
    public void WriteInt(int address, int value);
    public int this[int address] { get; set; }
    public int[] Debugger_ReadAllIntMemoryAddresses();
    public int MemorySize { get; }
    string ReadStr(int memAddress);
    void WriteStr(int memAddress, string value);
}