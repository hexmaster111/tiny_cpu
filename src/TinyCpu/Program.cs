
internal class Program
{
    static TinyCpu cpu = new();
    private static void Main(string[] args)
    {
        while (!cpu.Halt)
            cpu.Step();
    }
}

public class TinyCpu
{
    public CpuRegisters Reg = new();
    public byte[] Instructions = new byte[] { 0x01, 0x01, 0x01, 0x00 };

    public bool Halt = false;



    public void Step()
    {
        //first byte of the current instruction
        var currInstPtr = (OpCode)Instructions[Reg.INST_PTR];

        switch (currInstPtr)
        {
            case OpCode.NOOP:
                //Dose nothing
                break;
            case OpCode.SETREG_R_C:
                var register = Instructions[Reg.INST_PTR + 1];  //dst reg index
                var value = Instructions[Reg.INST_PTR + 2]; //src const value
                Reg.Data[register] = value; //We could do like a constants table and use value as its index
                break;
            default:
                throw new Exception($"Unknown OPCODE: {currInstPtr}");
        }


        Reg.Data[(int)RegisterIndex.INST_PTR] += currInstPtr.GetInstructionByteCount();

    }

}

public enum OpCode : byte
{
    NOOP = 0x00,
    SETREG_R_C = 0x01,
}

public struct CpuRegisters
{
    public CpuRegisters()
    {
        Data = new int[byte.MaxValue];
    }

    public readonly int[] Data;

    public readonly int INST_PTR => Data[(int)RegisterIndex.INST_PTR];

    public readonly int GP_I32_0 => Data[(int)RegisterIndex.GP_I32_0];


}
public enum RegisterIndex : byte
{
    INST_PTR = 0,
    GP_I32_0 = 1,
}