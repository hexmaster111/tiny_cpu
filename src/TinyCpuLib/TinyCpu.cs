namespace TinyCpuLib;

public class TinyCpu
{
    public const int MAX_STACK = 10;
    public CpuRegisters Reg = new();
    public Stack<int> CallStack = new();
    public Stack<int> ValueStack = new();

    public byte[] TCpuExe = Array.Empty<byte>();

    private int ReadInstructionIntAbs(int index) => BitConverter.ToInt32(TCpuExe, index);
    private byte ReadInstructionByteAbs(int index) => TCpuExe[index];
    private byte ReadInstructionByteRel(int ipOffset) => ReadInstructionByteAbs(Reg.INST_PTR + ipOffset);
    private int ReadInstructionIntRel(int ipOffset) => ReadInstructionIntAbs(Reg.INST_PTR + ipOffset);


    public void Step()
    {
        //first byte of the current instruction
        var currInst = (OpCode)ReadInstructionByteRel(0);

        switch (currInst)
        {
            case OpCode.NOOP:
                //Dose nothing
                break;
            case OpCode.SETREG_R_C:
            {
                var register = ReadInstructionByteRel(1);
                Reg.Data[register] = ReadInstructionIntRel(2);
            }
                break;
            case OpCode.HALT:
                Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.F0_HALT, true);
                break;
            case OpCode.SETREG_R_R:
            {
                var dest = ReadInstructionByteRel(1); // reg index
                var src = ReadInstructionByteRel(2); //reg index
                Reg.Data[dest] = Reg.Data[src];
            }
                break;
            case OpCode.ADD_R_C:
            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Data[dest] += constVal;
            }
                break;
            case OpCode.ADD_R_R:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Data[dest] += Reg.Data[src];
            }
                break;
            case OpCode.MUL_R_C:

            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Data[dest] *= constVal;
            }
                break;
            case OpCode.MUL_R_R:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Data[dest] *= Reg.Data[src];
            }
                break;
            case OpCode.SUB_R_C:

            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Data[dest] -= constVal;
            }
                break;
            case OpCode.SUB_R_R:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Data[dest] -= Reg.Data[src];
            }
                break;
            case OpCode.DIV_R_C:

            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Data[dest] /= constVal;
            }
                break;
            case OpCode.DIV_R_R:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Data[dest] /= Reg.Data[src];
            }
                break;
            case OpCode.CALL_C:
            {
                var destAdress = ReadInstructionIntRel(1);
                CallInternal(destAdress, Reg.Data[(int)RegisterIndex.INST_PTR] + currInst.GetInstructionByteCount());
                return; //Modifys inst ptr directly (no inc)
            }
            case OpCode.CALL_R:
            {
                var destAdressReg = ReadInstructionByteRel(1);
                var destAdress = Reg.Data[destAdressReg];
                CallInternal(destAdress, Reg.Data[(int)RegisterIndex.INST_PTR] + currInst.GetInstructionByteCount());
                return; //Modifys inst ptr directly (no inc)
            }
            case OpCode.RET:
                RetInternal(); //Modifys inst ptr directly (no inc)
                return;
            case OpCode.CALL_D:
                break;
            case OpCode.PUSH_C:
            {
                var val = ReadInstructionIntRel(1);
                PushValueStack(val);
                break;
            }
            case OpCode.PUSH_R:
            {
                var valSrc = (RegisterIndex) ReadInstructionByteRel(1);
                var val = Reg.Data[(int)valSrc];
                PushValueStack(val);
                break;
            }
            case OpCode.POP_R:
            {
                var destReg = (RegisterIndex)ReadInstructionByteRel(1);
                Reg.Data[(int)destReg] = ValueStack.Pop();
                break;
            }
            default:
                throw new Exception($"Unknown OPCODE: {currInst}");
        }

        Reg.Data[(int)RegisterIndex.INST_PTR] += currInst.GetInstructionByteCount();
    }

    private void PushValueStack(int val)
    {
        ValueStack.Push(val);
        if (ValueStack.Count > MAX_STACK)
            throw new Exception("Value Stack Smashed!");
    }


    private void RetInternal()
    {
        if (!CallStack.Any()) throw new Exception("Attempt to ret without call");

        var retAdress = CallStack.Pop();

        if (retAdress > TCpuExe.Length)
            throw new Exception("Call to invalid location (destnation off file)");

        Reg.Data[(int)RegisterIndex.INST_PTR] = retAdress;
    }

    private void CallInternal(int callAdress, int nextAddress)
    {
        if (callAdress > TCpuExe.Length)
            throw new Exception("Call to invalid location (destnation off file)");

        var instAtAdress = TCpuExe[callAdress];

        if (instAtAdress != (byte)OpCode.CALL_D)
            throw new Exception("Call to invalid location (didnt hit call_d)");

        CallStack.Push(nextAddress);
        if (CallStack.Count() > MAX_STACK)
            throw new Exception("Stack smashed");

        Reg.Data[(int)RegisterIndex.INST_PTR] = callAdress;
    }

    public void DumpState()
    {
        Console.Clear();
        Console.WriteLine("-----------------------");
        Console.WriteLine($"{nameof(CpuRegisters.INST_PTR)}:{Reg.INST_PTR:X4}");
        Console.WriteLine($"{nameof(CpuRegisters.GP_I32_0)}:{Reg.GP_I32_0:X4}");
        Console.WriteLine($"{nameof(CpuRegisters.GP_I32_1)}:{Reg.GP_I32_1:X4}");
        Console.WriteLine($"{nameof(CpuRegisters.GP_I32_2)}:{Reg.GP_I32_2:X4}");

        var currInst = (OpCode)ReadInstructionByteRel(0);
        Console.WriteLine($"Current Instruction :{currInst}");
    }

    public void LoadProgram(byte[] bytes) => TCpuExe = bytes;
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
    PUSH_C = 0xA0,
    PUSH_R = 0xA1,
    POP_R = 0xA2,
    CALL_C = 0xA3,
    CALL_R = 0xA4,
    RET = 0xA5,
    CALL_D = 0xA6,

    HALT = 0xFF,
}

public static class Ext
{
    public static int GetInstructionByteCount(this OpCode c) => c switch
    {
        OpCode.NOOP => 1, //Opcode
        OpCode.HALT => 1, //Opcode
        OpCode.RET => 1, //Opcode
        OpCode.CALL_D => 1, //Opcode
        OpCode.SETREG_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.ADD_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.MUL_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.SUB_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.DIV_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.ADD_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.SETREG_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.MUL_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.SUB_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.DIV_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.CALL_C => 1 + 4, //Opcode + int
        OpCode.CALL_R => 1 + 1, //Opcode + byte
        OpCode.PUSH_C => 1 + 4, //Opcode + int
        OpCode.PUSH_R => 1 + 1, //Opcode + byte
        OpCode.POP_R => 1 + 1, //Opcode + byte
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