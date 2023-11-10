
using System.Collections;
using System.Diagnostics.Contracts;

internal class Program
{
    static TinyCpu cpu = new();
    private static void Main(string[] args)
    {
        while (!cpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.F0_HALT))
        {
            cpu.Step();
        }


    }
}

public class TinyCpu
{
    public CpuRegisters Reg = new();
    public byte[] Instructions = new byte[] {
        0x01, 0x04, 0x01, 0x00, 0x00, 0x00, //SETREG_R_C GP_I_0 1
        0x02, 0x05, 0x04,                   //SETREG_R_R GP_I_0 GP_I_1
        0x00,                               //NOOP
        0x03, 0x05, 0x02, 0x00, 0x00, 0x00, //ADD_R_C GP_I_1 2
        0xA5                                //HALT
    };

    private int ReadInstrucionIntAbs(int index) => BitConverter.ToInt32(Instructions, index);
    private byte ReadInstrucionByteAbs(int index) => Instructions[index];
    private byte ReadInstrucionByteRel(int ipOffset) => ReadInstrucionByteAbs(Reg.INST_PTR + ipOffset);
    private int ReadInstrucionIntRel(int ipOffset) => ReadInstrucionIntAbs(Reg.INST_PTR + ipOffset);


    public void Step()
    {
        //first byte of the current instruction
        var currInst = (OpCode)Instructions[Reg.INST_PTR];

        switch (currInst)
        {
            case OpCode.NOOP:
                //Dose nothing
                break;
            case OpCode.SETREG_R_C:
                {
                    var register = ReadInstrucionByteRel(1);
                    Reg.Data[register] = ReadInstrucionIntRel(2);
                }
                break;
            case OpCode.HALT:
                Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.F0_HALT, true);
                break;
            case OpCode.SETREG_R_R:
                {

                    var dest = ReadInstrucionByteRel(1);  // reg index
                    var src = ReadInstrucionByteRel(2);  //reg index
                    Reg.Data[dest] = Reg.Data[src];
                }
                break;
            case OpCode.ADD_R_C:
                {
                    var dest = ReadInstrucionByteRel(1);
                    var constVal = ReadInstrucionIntRel(2);
                    Reg.Data[dest] += constVal;
                }
                break;
            case OpCode.ADD_R_R:
                {

                    var dest = ReadInstrucionByteRel(1);
                    var src = ReadInstrucionByteRel(2);
                    Reg.Data[dest] += Reg.Data[src];
                }
                break;
            case OpCode.MUL_R_C:

                {
                    var dest = ReadInstrucionByteRel(1);
                    var constVal = ReadInstrucionIntRel(2);
                    Reg.Data[dest] *= constVal;
                }
                break;
            case OpCode.MUL_R_R:
                {

                    var dest = ReadInstrucionByteRel(1);
                    var src = ReadInstrucionByteRel(2);
                    Reg.Data[dest] *= Reg.Data[src];
                }
                break;
            case OpCode.SUB_R_C:

                {
                    var dest = ReadInstrucionByteRel(1);
                    var constVal = ReadInstrucionIntRel(2);
                    Reg.Data[dest] -= constVal;
                }
                break;
            case OpCode.SUB_R_R:
                {

                    var dest = ReadInstrucionByteRel(1);
                    var src = ReadInstrucionByteRel(2);
                    Reg.Data[dest] -= Reg.Data[src];
                }
                break;
            case OpCode.DIV_R_C:

                {
                    var dest = ReadInstrucionByteRel(1);
                    var constVal = ReadInstrucionIntRel(2);
                    Reg.Data[dest] /= constVal;
                }
                break;
            case OpCode.DIV_R_R:
                {

                    var dest = ReadInstrucionByteRel(1);
                    var src = ReadInstrucionByteRel(2);
                    Reg.Data[dest] /= Reg.Data[src];
                }
                break;
            default:
                throw new Exception($"Unknown OPCODE: {currInst}");
        }

        Reg.Data[(int)RegisterIndex.INST_PTR] += currInst.GetInstructionByteCount();

    }

}

public enum OpCode : byte
{
    NOOP = 0x00,
    SETREG_R_C = 0x01,
    SETREG_R_R = 0x02,
    ADD_R_C = 0x03,
    ADD_R_R = 0x04,
    MUL_R_C = 0x05,
    MUL_R_R = 0x06,
    SUB_R_C = 0x07,
    SUB_R_R = 0x08,
    DIV_R_C = 0x09,
    DIV_R_R = 0x0A,
    HALT = 0xA5,
}

public static class Ext
{
    public static int GetInstructionByteCount(this OpCode c) => c switch
    {
        OpCode.NOOP => 1, //Opcode
        OpCode.SETREG_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.SETREG_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.HALT => 1,            //Opcode
        OpCode.ADD_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.ADD_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.MUL_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.MUL_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.SUB_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.SUB_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.DIV_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.DIV_R_R => 1 + 1 + 1, //Opcode + byte + byte
        _ => throw new Exception($"Unknown OPCODE: {c}"),
    };

    public static bool ReadBit(this int flags, int bitIndex) => (flags & (1 << bitIndex)) != 0;


    public static void SetBit(this ref int flags, int bitIndex, bool newValue)
    {
        if (newValue)
        {
            flags |= (1 << bitIndex);
        }
        else
        {
            flags &= ~(1 << bitIndex);
        }
    }
}


public enum FLAGS_0_USAGE
{
    F0_HALT = 0,
}

public enum RegisterIndex : byte
{
    INST_PTR = 0,
    FLAGS_0 = 1,
    RESERVED_0 = 2,
    RESERVED_1 = 3,
    GP_I32_0 = 4,
    GP_I32_1 = 5,
    GP_I32_2 = 6,
}


public struct CpuRegisters
{
    public CpuRegisters()
    {
        Data = new int[byte.MaxValue];
    }

    public readonly int[] Data;

    public readonly int INST_PTR => Data[(int)RegisterIndex.INST_PTR];
    public readonly int FLAGS_0 => Data[(int)RegisterIndex.FLAGS_0];
    public readonly int RESERVED_0 => Data[(int)RegisterIndex.RESERVED_0];
    public readonly int RESERVED_1 => Data[(int)RegisterIndex.RESERVED_1];
    public readonly int GP_I32_0 => Data[(int)RegisterIndex.GP_I32_0];
    public readonly int GP_I32_1 => Data[(int)RegisterIndex.GP_I32_1];
    public readonly int GP_I32_2 => Data[(int)RegisterIndex.GP_I32_2];

}

