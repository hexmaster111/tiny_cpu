namespace TinyCpuLib;

public class TinyCpu
{
    public byte[] TCpuExe = Array.Empty<byte>();
    public IMemory Memory = new VirtualMemory();


    private const int MAX_STACK = 10;

    public readonly CpuRegisters Reg = new();
    public readonly Stack<int> CallStack = new();
    public readonly Stack<int> ValueStack = new();
    public UInt128 Cycles { get; private set; } = 0;

    /// <summary>
    ///     Runtime hondered cycle time
    /// </summary>
    public int CycleTimeHz { get; set; } = 1;

    private int ReadInstructionIntAbs(int index) => BitConverter.ToInt32(TCpuExe, index);
    private byte ReadInstructionByteAbs(int index) => TCpuExe[index];
    public byte ReadInstructionByteRel(int ipOffset) => ReadInstructionByteAbs(Reg.INST_PTR + ipOffset);
    private int ReadInstructionIntRel(int ipOffset) => ReadInstructionIntAbs(Reg.INST_PTR + ipOffset);

    private RegisterIndex ReadInstructionRegisterIndexByteRel(int ipOffset) =>
        (RegisterIndex)ReadInstructionByteRel(ipOffset);

    public void Step()
    {
        Cycles++;
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
                Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.HALT, true); 
                return;
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
                var destAddress = ReadInstructionIntRel(1);
                CallInternal(destAddress, Reg.Data[(int)RegisterIndex.INST_PTR] + currInst.GetInstructionByteCount());
                return; //Modify inst ptr directly (no inc)
            }
            case OpCode.CALL_R:
            {
                var destAddressReg = ReadInstructionByteRel(1);
                var destAddress = Reg.Data[destAddressReg];
                CallInternal(destAddress, Reg.Data[(int)RegisterIndex.INST_PTR] + currInst.GetInstructionByteCount());
                return; //Modify inst ptr directly (no inc)
            }
            case OpCode.RET:
                RetInternal(); //Modify inst ptr directly (no inc)
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
                var valSrc = ReadInstructionRegisterIndexByteRel(1);
                PushValueStack(Reg[valSrc]);
                break;
            }
            case OpCode.POP_R:
            {
                var destReg = (RegisterIndex)ReadInstructionByteRel(1);
                Reg.Data[(int)destReg] = ValueStack.Pop();
                break;
            }
            case OpCode.INC:
            {
                var destReg = (RegisterIndex)ReadInstructionByteRel(1);
                Reg.Data[(int)destReg] += 1;
                break;
            }
            case OpCode.DEC:
            {
                var destReg = (RegisterIndex)ReadInstructionByteRel(1);
                Reg.Data[(int)destReg] -= 1;
                break;
            }
            case OpCode.CMP_R_C:
            {
                var regA = Reg.Data[(int)(RegisterIndex)ReadInstructionByteRel(1)];
                var constValeB = ReadInstructionIntRel(2);
                CmpInternal(regA, constValeB);
                break;
            }
            case OpCode.CMP_R_R:
            {
                var regA = Reg.Data[(int)(RegisterIndex)ReadInstructionByteRel(1)];
                var regB = Reg.Data[(int)(RegisterIndex)ReadInstructionByteRel(2)];
                CmpInternal(regA, regB);
                break;
            }
            case OpCode.JMP_C_EQ:
            case OpCode.JMP_C_NEQ:
            case OpCode.JMP_C_GTR:
            case OpCode.JMP_C_GEQ:
            case OpCode.JMP_C_LES:
            case OpCode.JMP_C_LEQ:
                if (JmpInternal(currInst, ReadInstructionIntRel(1)))
                    return; //Modifes Inst Ptr 
                break;
            case OpCode.JMP_R_EQ:
            case OpCode.JMP_R_NEQ:
            case OpCode.JMP_R_GTR:
            case OpCode.JMP_R_GEQ:
            case OpCode.JMP_R_LES:
            case OpCode.JMP_R_LEQ:
                if (JmpRegInternal(currInst, (RegisterIndex)ReadInstructionByteRel(1)))
                    return; //Modifies Inst Ptr 
                break;
            case OpCode.JMP_R:
                Reg.Data[(int)RegisterIndex.INST_PTR] =
                    Reg.Data[(int)(RegisterIndex)ReadInstructionByteAbs(1)];
                return;
            case OpCode.JMP_C:
                Reg.Data[(int)RegisterIndex.INST_PTR] = ReadInstructionIntRel(1);
                return;

            case OpCode.MEM_READ_R_C:
            {
                var destReg = ReadInstructionRegisterIndexByteRel(1);
                var memAddress = ReadInstructionIntRel(2);
                Reg[destReg] = Memory.Read(memAddress);
            }
                break;
            case OpCode.MEM_READ_R_R:
            {
                var destReg = ReadInstructionRegisterIndexByteRel(1);
                var srcReg = ReadInstructionRegisterIndexByteRel(2);
                Reg[destReg] = Memory.Read(Reg[srcReg]);
            }
                break;
            case OpCode.MEM_WRITE_R_C:
            {
                var valReg = ReadInstructionRegisterIndexByteRel(1);
                var writeAddress = ReadInstructionIntRel(2);
                Memory.Write(writeAddress, Reg[valReg]);
            }
                break;
            case OpCode.MEM_WRITE_R_R:
            {
                var valReg = ReadInstructionRegisterIndexByteRel(1);
                var writeRegAddr = ReadInstructionRegisterIndexByteRel(2);
                Memory.Write(Reg[writeRegAddr], Reg[valReg]);
            }
                break;

            default:
                throw new Exception($"Unknown OPCODE: {currInst}");
        }

        Reg.Data[(int)RegisterIndex.INST_PTR] += currInst.GetInstructionByteCount();
    }

    private bool JmpRegInternal(OpCode currInst, RegisterIndex readInstructionByteRel) =>
        JmpInternal(currInst, Reg.Data[(int)readInstructionByteRel]);


    private bool JmpInternal(OpCode currInst, int jmpAddress)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault -
        //This only applies to the JMP instructions
        var jmp = currInst switch
        {
            OpCode.JMP_C_EQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.EQ),
            OpCode.JMP_R_EQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.EQ),
            OpCode.JMP_C_GTR => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GTR),
            OpCode.JMP_R_GTR => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GTR),
            OpCode.JMP_C_LEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LEQ),
            OpCode.JMP_R_LEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LEQ),
            OpCode.JMP_R_NEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.NEQ),
            OpCode.JMP_C_NEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.NEQ),
            OpCode.JMP_C_GEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GEQ),
            OpCode.JMP_R_GEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GEQ),
            OpCode.JMP_R_LES => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LES),
            OpCode.JMP_C_LES => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LES),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (jmp) Reg.Data[(int)RegisterIndex.INST_PTR] = jmpAddress;
        return jmp;
    }

    private void CmpInternal(int a, int b)
    {
        Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.EQ, a == b);
        Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.NEQ, a != b);
        Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.GTR, a > b);
        Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.GEQ, a >= b);
        Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.LES, a < b);
        Reg.Data[(int)RegisterIndex.FLAGS_0].SetBit((int)FLAGS_0_USAGE.LEQ, a <= b);
    }

    private void PushValueStack(int val)
    {
        ValueStack.Push(val);
        if (ValueStack.Count > MAX_STACK) throw new Exception("Value Stack Smashed!");
    }


    private void RetInternal()
    {
        if (!CallStack.Any()) throw new Exception("Attempt to ret without call");

        var retAdress = CallStack.Pop();

        if (retAdress > TCpuExe.Length)
            throw new Exception("Call to invalid location (dst off file)");

        Reg.Data[(int)RegisterIndex.INST_PTR] = retAdress;
    }

    private void CallInternal(int callAdress, int nextAddress)
    {
        if (callAdress > TCpuExe.Length)
            throw new Exception("Call to invalid location (dst off file)");

        var instAtAdress = TCpuExe[callAdress];

        if (instAtAdress != (byte)OpCode.CALL_D)
            throw new Exception("Call to invalid location (didnt hit call_d)");

        CallStack.Push(nextAddress);
        if (CallStack.Count() > MAX_STACK)
            throw new Exception("Stack smashed");

        Reg.Data[(int)RegisterIndex.INST_PTR] = callAdress;
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
    INC = 0x0B,
    DEC = 0x0C,
    CMP_R_C = 0x0D,
    CMP_R_R = 0x0E,

    //0F
    PUSH_C = 0xA0,
    PUSH_R = 0xA1,
    POP_R = 0xA2,
    CALL_C = 0xA3,
    CALL_R = 0xA4,
    RET = 0xA5,
    CALL_D = 0xA6,

    JMP_C_EQ = 0xA7,
    JMP_C_NEQ = 0xA8,
    JMP_C_GTR = 0xA9,
    JMP_C_GEQ = 0xAA,
    JMP_C_LES = 0xAB,
    JMP_C_LEQ = 0xAC,
    JMP_R_EQ = 0xAD,
    JMP_R_NEQ = 0xAE,
    JMP_R_GTR = 0xAF,
    JMP_R_GEQ = 0xB0,
    JMP_R_LES = 0xB1,
    JMP_R_LEQ = 0xB2,
    JMP_R = 0xB3,
    JMP_C = 0xB4,

    MEM_READ_R_C = 0xB5,
    MEM_READ_R_R = 0xB6,
    MEM_WRITE_R_C = 0xB7,
    MEM_WRITE_R_R = 0xB8,

    HALT = 0xFF,
}

public static class Ext
{
    public static string MemoryDump(this int[] memory)
    {
        const int ADDR_PER_LINE = 8;
        var addr = 0;
        var o = "";
        foreach (var memVal in memory)
        {
            o += $"{addr++:X2}:{memVal:X4} ";
            if (addr % ADDR_PER_LINE == 0) o += Environment.NewLine;
        }

        return o;
    }

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
        OpCode.INC => 1 + 1, //Opcode + byte
        OpCode.DEC => 1 + 1, //Opcode + byte
        OpCode.CMP_R_C => 1 + 1 + 4, //Opcode + byte + int
        OpCode.CMP_R_R => 1 + 1 + 1, //Opcode + byte + byte
        OpCode.JMP_C_EQ => 1 + 4, //Opcode + Int
        OpCode.JMP_C_NEQ => 1 + 4, //Opcode + Int
        OpCode.JMP_C_GTR => 1 + 4, //Opcode + Int
        OpCode.JMP_C_GEQ => 1 + 4, //Opcode + Int
        OpCode.JMP_C_LES => 1 + 4, //Opcode + Int
        OpCode.JMP_C_LEQ => 1 + 4, //Opcode + Int
        OpCode.JMP_R_EQ => 1 + 1, //Opcode + byte
        OpCode.JMP_R_NEQ => 1 + 1, //Opcode + byte
        OpCode.JMP_R_GTR => 1 + 1, //Opcode + byte
        OpCode.JMP_R_GEQ => 1 + 1, //Opcode + byte
        OpCode.JMP_R_LES => 1 + 1, //Opcode + byte
        OpCode.JMP_R_LEQ => 1 + 1, //Opcode + byte
        OpCode.JMP_R => 1 + 1, // Opcode byte
        OpCode.JMP_C => 1 + 4, //opcode int
        OpCode.MEM_READ_R_C => 1 + 1 + 4, //opcode byte int
        OpCode.MEM_READ_R_R => 1 + 1 + 1, //opcode byte byte
        OpCode.MEM_WRITE_R_C => 1 + 1 + 4, //opcode byte int
        OpCode.MEM_WRITE_R_R => 1 + 1 + 1, //opcode byte byte
        _ => throw new Exception($"Unknown OPCODE: {c}"),
    };

    public static bool ReadBit(this int flags, int bitIndex) => (flags & (1 << bitIndex)) != 0;
    public static bool ReadBit(this int flags, FLAGS_0_USAGE flag) => ReadBit(flags, (int)flag);

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
    HALT = 0,
    EQ = 1,
    NEQ = 2,
    GTR = 3,
    GEQ = 4,
    LES = 5,
    LEQ = 6
}

public enum RegisterIndex
{
    INST_PTR = 0,
    FLAGS_0 = 1,
    RESERVED_0 = 2,
    RESERVED_1 = 3,
    GP_I32_0 = 4,
    GP_I32_1 = 5,
    GP_I32_2 = 6,
}

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

public readonly struct CpuRegisters
{
    public CpuRegisters() => Data = new int[Enum.GetValues(typeof(RegisterIndex)).Length];
    public readonly int[] Data;
    public int INST_PTR => Data[(int)RegisterIndex.INST_PTR];
    public int FLAGS_0 => Data[(int)RegisterIndex.FLAGS_0];
    public bool FLAGS_0_HALT => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT);
    public bool FLAGS_0_EQ => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.EQ);
    public bool FLAGS_0_NEQ => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.NEQ);
    public bool FLAGS_0_GTR => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GTR);
    public bool FLAGS_0_GEQ => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GEQ);
    public bool FLAGS_0_LES => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LES);
    public bool FLAGS_0_LEQ => FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LEQ);

    public int RESERVED_0 => Data[(int)RegisterIndex.RESERVED_0];
    public int RESERVED_1 => Data[(int)RegisterIndex.RESERVED_1];
    public int GP_I32_0 => Data[(int)RegisterIndex.GP_I32_0];
    public int GP_I32_1 => Data[(int)RegisterIndex.GP_I32_1];
    public int GP_I32_2 => Data[(int)RegisterIndex.GP_I32_2];


    public int this[byte key]
    {
        get => GetRegisterValue(key);
        set => SetRegisterValue(key, value);
    }

    public int this[RegisterIndex key]
    {
        get => GetRegisterValue(key);
        set => SetRegisterValue(key, value);
    }

    public void SetRegisterValue(RegisterIndex register, int value) => SetRegisterValue((byte)register, value);

    public int GetRegisterValue(RegisterIndex register) => GetRegisterValue((byte)register);

    private void SetRegisterValue(byte register, int value) => Data[register] = value;

    private int GetRegisterValue(byte register) => Data[register];
}