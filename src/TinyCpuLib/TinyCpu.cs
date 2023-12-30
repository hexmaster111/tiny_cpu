using System.Text;

namespace TinyCpuLib;

// @formatter:place_simple_accessorholder_on_single_line true
// @formatter:place_expr_accessor_on_single_line true
// @formatter:instance_members_qualify_declared_in base_class
// @formatter:accessor_owner_body accessors_with_expression_body
public class TinyCpu
{
    public byte[] TCpuExe = Array.Empty<byte>();
    public IMemory Memory = new VirtualMemory();


    private const int MAX_STACK = 10;

    public readonly CpuRegisters Reg = new();
    public readonly Stack<int> CallStack = new();
    public readonly Stack<int> IntValueStack = new();
    public readonly Stack<string> StrValueStack = new();
    public UInt128 Cycles { get; private set; } = 0;

    /// <summary>
    ///     Runtime hondered cycle time
    /// </summary>
    public int CycleTimeHz { get; set; } = 1;

    private int ReadInstructionIntAbs(int index) => BitConverter.ToInt32(TCpuExe, index);
    private byte ReadInstructionByteAbs(int index) => TCpuExe[index];
    public byte ReadInstructionByteRel(int ipOffset) => ReadInstructionByteAbs(Reg.INST_PTR + ipOffset);
    private int ReadInstructionIntRel(int ipOffset) => ReadInstructionIntAbs(Reg.INST_PTR + ipOffset);

    private IntRegisterIndex ReadInstIntRegIndexByteRel(int ipOffset) =>
        (IntRegisterIndex)ReadInstructionByteRel(ipOffset);

    private StrRegisterIndex ReadInstStrRegIndexRel(int ipOffset) =>
        (StrRegisterIndex)ReadInstructionByteRel(ipOffset);

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
            case OpCode.SETREG_INTR_INTC:
            {
                var register = ReadInstructionByteRel(1);
                Reg.Int[register] = ReadInstructionIntRel(2);
            }
                break;
            case OpCode.HALT:
                Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.HALT, true);
                return;
            case OpCode.SETREG_INTR_INTR:
            {
                var dest = ReadInstructionByteRel(1); // reg index
                var src = ReadInstructionByteRel(2); //reg index
                Reg.Int[dest] = Reg.Int[src];
            }
                break;
            case OpCode.ADD_INTR_INTC:
            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Int[dest] += constVal;
            }
                break;
            case OpCode.ADD_INTR_INTR:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Int[dest] += Reg.Int[src];
            }
                break;
            case OpCode.MUL_INTR_INTC:
            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Int[dest] *= constVal;
            }
                break;
            case OpCode.MUL_INTR_INTR:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Int[dest] *= Reg.Int[src];
            }
                break;
            case OpCode.SUB_INTR_INTC:
            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Int[dest] -= constVal;
            }
                break;
            case OpCode.SUB_INTR_INTR:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Int[dest] -= Reg.Int[src];
            }
                break;
            case OpCode.DIV_INTR_INTC:
            {
                var dest = ReadInstructionByteRel(1);
                var constVal = ReadInstructionIntRel(2);
                Reg.Int[dest] /= constVal;
            }
                break;
            case OpCode.DIV_INTR_INTR:
            {
                var dest = ReadInstructionByteRel(1);
                var src = ReadInstructionByteRel(2);
                Reg.Int[dest] /= Reg.Int[src];
            }
                break;
            case OpCode.CALL_INTC:
            {
                var destAddress = ReadInstructionIntRel(1);
                CallInternal(destAddress, Reg.Int[(int)IntRegisterIndex.INST_PTR] + currInst.GetInstructionByteCount());
                return; //Modify inst ptr directly (no inc)
            }
            case OpCode.CALL_INTR:
            {
                var destAddressReg = ReadInstructionByteRel(1);
                var destAddress = Reg.Int[destAddressReg];
                CallInternal(destAddress, Reg.Int[(int)IntRegisterIndex.INST_PTR] + currInst.GetInstructionByteCount());
                return; //Modify inst ptr directly (no inc)
            }
            case OpCode.RET:
                RetInternal(); //Modify inst ptr directly (no inc)
                return;
            case OpCode.CALLD:
                break;
            case OpCode.PUSH_INTC:
            {
                var val = ReadInstructionIntRel(1);
                PushValueStack(val);
                break;
            }
            case OpCode.PUSH_INTR:
            {
                var valSrc = ReadInstIntRegIndexByteRel(1);
                PushValueStack(Reg[valSrc]);
                break;
            }
            case OpCode.POP_INTR:
            {
                var destReg = (IntRegisterIndex)ReadInstructionByteRel(1);
                Reg.Int[(int)destReg] = IntValueStack.Pop();
                break;
            }
            case OpCode.INC_INTR:
            {
                var destReg = (IntRegisterIndex)ReadInstructionByteRel(1);
                Reg.Int[(int)destReg] += 1;
                break;
            }
            case OpCode.DEC_INTR:
            {
                var destReg = (IntRegisterIndex)ReadInstructionByteRel(1);
                Reg.Int[(int)destReg] -= 1;
                break;
            }
            case OpCode.CMP_INTR_INTC:
            {
                var regA = Reg.Int[(int)(IntRegisterIndex)ReadInstructionByteRel(1)];
                var constValeB = ReadInstructionIntRel(2);
                CmpInternal(regA, constValeB);
                break;
            }
            case OpCode.CMP_INTR_INTR:
            {
                var regA = Reg.Int[(int)(IntRegisterIndex)ReadInstructionByteRel(1)];
                var regB = Reg.Int[(int)(IntRegisterIndex)ReadInstructionByteRel(2)];
                CmpInternal(regA, regB);
                break;
            }
            case OpCode.JMP_INTC_EQ:
            case OpCode.JMP_INTC_NEQ:
            case OpCode.JMP_INTC_GTR:
            case OpCode.JMP_INTC_GEQ:
            case OpCode.JMP_INTC_LES:
            case OpCode.JMP_INTC_LEQ:
                if (JmpInternal(currInst, ReadInstructionIntRel(1)))
                    return; //Modifes Inst Ptr 
                break;
            case OpCode.JMP_INTR_EQ:
            case OpCode.JMP_INTR_NEQ:
            case OpCode.JMP_INTR_GTR:
            case OpCode.JMP_INTR_GEQ:
            case OpCode.JMP_INTR_LES:
            case OpCode.JMP_INTR_LEQ:
                if (JmpRegInternal(currInst, (IntRegisterIndex)ReadInstructionByteRel(1)))
                    return; //Modifies Inst Ptr 
                break;
            case OpCode.JMP_INTR:
                Reg.Int[(int)IntRegisterIndex.INST_PTR] =
                    Reg.Int[(int)(IntRegisterIndex)ReadInstructionByteAbs(1)];
                return;
            case OpCode.JMP_INTC:
                Reg.Int[(int)IntRegisterIndex.INST_PTR] = ReadInstructionIntRel(1);
                return;

            case OpCode.MEM_READ_INTR_INTC:
            {
                var destReg = ReadInstIntRegIndexByteRel(1);
                var memAddress = ReadInstructionIntRel(2);
                Reg[destReg] = Memory.ReadInt(memAddress);
                break;
            }
            case OpCode.MEM_READ_INTR_INTR:
            {
                var destReg = ReadInstIntRegIndexByteRel(1);
                var srcReg = ReadInstIntRegIndexByteRel(2);
                Reg[destReg] = Memory.ReadInt(Reg[srcReg]);
                break;
            }
            case OpCode.MEM_WRITE_INTR_INTC:
            {
                var valReg = ReadInstIntRegIndexByteRel(1);
                var writeAddress = ReadInstructionIntRel(2);
                Memory.WriteInt(writeAddress, Reg[valReg]);
                break;
            }
            case OpCode.MEM_WRITE_INTR_INTR:
            {
                var valReg = ReadInstIntRegIndexByteRel(1);
                var writeRegAddr = ReadInstIntRegIndexByteRel(2);
                Memory.WriteInt(Reg[writeRegAddr], Reg[valReg]);
                break;
            }

            case OpCode.SETREG_STRR_STRC:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var str = ReadInstStrRel(2);
                Reg.Str[(int)destReg] = str;
                Reg.Int[(int)IntRegisterIndex.INST_PTR] += str.Length + 3;
                return;
            }
            case OpCode.SETREG_STRR_STRR:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var srcReg = ReadInstStrRegIndexRel(2);
                Reg.Str[(int)destReg] = Reg.Str[(int)srcReg];
                break;
            }
            case OpCode.CCAT_STRR_STRR:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var srcReg = ReadInstStrRegIndexRel(2);
                Reg.Str[(int)destReg] += Reg.Str[(int)srcReg];
                break;
            }
            case OpCode.CCAT_STRR_STRC:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var srcStr = ReadInstStrRel(2);
                Reg.Str[(int)destReg] += srcStr;
                break;
            }
            case OpCode.CMP_STRR_STRR:
            {
                var regA = Reg.Str[(int)ReadInstStrRegIndexRel(1)];
                var regB = Reg.Str[(int)ReadInstStrRegIndexRel(2)];
                StrCmpInternal(regA, regB);
                break;
            }
            case OpCode.CMP_STRR_STRC:
            {
                var regA = Reg.Str[(int)ReadInstStrRegIndexRel(1)];
                var regB = ReadInstStrRel(2);
                StrCmpInternal(regA, regB);
                Reg.Int[(int)IntRegisterIndex.INST_PTR] += regB.Length + 3;
                return;
            }
            case OpCode.STRSPLT_STRR_INTC:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var start = ReadInstructionIntRel(2);
                var str = Reg.Str[(int)destReg];
                Reg.Str[(int)destReg] = str.Substring(start);
                break;
            }
            case OpCode.STRSPLT_STRR_INTR:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var partWanted = ReadInstructionIntRel(2);
                var splitChar = Reg.Str[0];
                var str = Reg.Str[(int)destReg];
                var parts = str.Split(splitChar);
                //TODO: cpu should have exception handling
                if (partWanted >= parts.Length) throw new Exception("Invalid part wanted");
                Reg.Str[(int)destReg] = parts[partWanted];
                break;
            }

            case OpCode.MEM_READ_STRR_INTC:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var memAddress = ReadInstructionIntRel(2);
                Reg.Str[(int)destReg] = Memory.ReadStr(memAddress);
                break;
            }
            case OpCode.MEM_READ_STRR_INTR:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                var srcReg = ReadInstIntRegIndexByteRel(2);
                Reg.Str[(int)destReg] = Memory.ReadStr(Reg[srcReg]);
                break;
            }
            case OpCode.MEM_WRITE_STRR_INTC:
            {
                var valReg = ReadInstStrRegIndexRel(1);
                var writeAddress = ReadInstructionIntRel(2);
                Memory.WriteStr(writeAddress, Reg.Str[(int)valReg]);
                break;
            }
            case OpCode.MEM_WRITE_STRR_INTR:
            {
                var valReg = ReadInstStrRegIndexRel(1);
                var writeRegAddr = ReadInstIntRegIndexByteRel(2);
                Memory.WriteStr(Reg[writeRegAddr], Reg.Str[(int)valReg]);
                break;
            }
            case OpCode.PUSH_STRR:
            {
                var valReg = ReadInstStrRegIndexRel(1);
                StrValueStack.Push(Reg.Str[(int)valReg]);
                break;
            }
            case OpCode.PUSH_STRC:
            {
                var str = ReadInstStrRel(1);
                StrValueStack.Push(str);
                Reg.Int[(int)IntRegisterIndex.INST_PTR] += str.Length + 2;
                return;
            }
            case OpCode.POP_STRR:
            {
                var destReg = ReadInstStrRegIndexRel(1);
                Reg.Str[(int)destReg] = StrValueStack.Pop();
                break;
            }

            default:
                throw new Exception($"Unknown OPCODE: {currInst}");
        }

        Reg.Int[(int)IntRegisterIndex.INST_PTR] += currInst.GetInstructionByteCount();
    }

    private void StrCmpInternal(string a, string b)
    {
        CmpInternal(a.Length, b.Length);

        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.EQ, a.SequenceEqual(b));
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.NEQ,
            !Reg.Int[(int)IntRegisterIndex.FLAGS_0].ReadBit((int)FLAGS_0_USAGE.EQ));
    }

    private string ReadInstStrRel(int ipOffset) => ReadInstructionStrAbs(ipOffset + Reg.INST_PTR);

    private string ReadInstructionStrAbs(int startAddr)
    {
        var str = "";
        for (var i = startAddr; i < TCpuExe.Length; i++)
        {
            var currChar = TCpuExe[i];
            if (currChar == 0x00) break;
            str += (char)currChar;
        }

        return str;
    }

    private bool JmpRegInternal(OpCode currInst, IntRegisterIndex readInstructionByteRel) =>
        JmpInternal(currInst, Reg.Int[(int)readInstructionByteRel]);


    private bool JmpInternal(OpCode currInst, int jmpAddress)
    {
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault -
        //This only applies to the JMP instructions
        var jmp = currInst switch
        {
            OpCode.JMP_INTC_EQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.EQ),
            OpCode.JMP_INTR_EQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.EQ),
            OpCode.JMP_INTC_GTR => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GTR),
            OpCode.JMP_INTR_GTR => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GTR),
            OpCode.JMP_INTC_LEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LEQ),
            OpCode.JMP_INTR_LEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LEQ),
            OpCode.JMP_INTR_NEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.NEQ),
            OpCode.JMP_INTC_NEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.NEQ),
            OpCode.JMP_INTC_GEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GEQ),
            OpCode.JMP_INTR_GEQ => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.GEQ),
            OpCode.JMP_INTR_LES => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LES),
            OpCode.JMP_INTC_LES => Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.LES),
            _ => throw new ArgumentOutOfRangeException()
        };

        if (jmp) Reg.Int[(int)IntRegisterIndex.INST_PTR] = jmpAddress;
        return jmp;
    }

    private void CmpInternal(int a, int b)
    {
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.EQ, a == b);
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.NEQ, a != b);
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.GTR, a > b);
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.GEQ, a >= b);
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.LES, a < b);
        Reg.Int[(int)IntRegisterIndex.FLAGS_0].WriteBit((int)FLAGS_0_USAGE.LEQ, a <= b);
    }

    private void PushValueStack(int val)
    {
        IntValueStack.Push(val);
        if (IntValueStack.Count > MAX_STACK) throw new Exception("Value Stack Smashed!");
    }


    private void RetInternal()
    {
        if (!CallStack.Any()) throw new Exception("Attempt to ret without call");

        var retAdress = CallStack.Pop();

        if (retAdress > TCpuExe.Length)
            throw new Exception("Call to invalid location (dst off file)");

        Reg.Int[(int)IntRegisterIndex.INST_PTR] = retAdress;
    }

    private void CallInternal(int callAdress, int nextAddress)
    {
        if (callAdress > TCpuExe.Length)
            throw new Exception("Call to invalid location (dst off file)");

        var instAtAdress = TCpuExe[callAdress];

        if (instAtAdress != (byte)OpCode.CALLD)
            throw new Exception("Call to invalid location (didnt hit call_d)");

        CallStack.Push(nextAddress);
        if (CallStack.Count() > MAX_STACK)
            throw new Exception("Stack smashed");

        Reg.Int[(int)IntRegisterIndex.INST_PTR] = callAdress;
    }


    public void LoadProgram(byte[] bytes) => TCpuExe = bytes;
}

public enum OpCode : byte
{
    NOOP = 0x00,
    SETREG_INTR_INTC = 0x01,
    SETREG_INTR_INTR = 0x02,
    ADD_INTR_INTC = 0x03,
    ADD_INTR_INTR = 0x04,
    MUL_INTR_INTC = 0x05,
    MUL_INTR_INTR = 0x06,
    SUB_INTR_INTC = 0x07,
    SUB_INTR_INTR = 0x08,
    DIV_INTR_INTC = 0x09,
    DIV_INTR_INTR = 0x0A,
    INC_INTR = 0x0B,
    DEC_INTR = 0x0C,
    CMP_INTR_INTC = 0x0D,
    CMP_INTR_INTR = 0x0E,

    //0F
    PUSH_INTC = 0xA0,
    PUSH_INTR = 0xA1,
    POP_INTR = 0xA2,
    CALL_INTC = 0xA3,
    CALL_INTR = 0xA4,
    RET = 0xA5,
    CALLD = 0xA6,

    JMP_INTC_EQ = 0xA7,
    JMP_INTC_NEQ = 0xA8,
    JMP_INTC_GTR = 0xA9,
    JMP_INTC_GEQ = 0xAA,
    JMP_INTC_LES = 0xAB,
    JMP_INTC_LEQ = 0xAC,
    JMP_INTR_EQ = 0xAD,
    JMP_INTR_NEQ = 0xAE,
    JMP_INTR_GTR = 0xAF,
    JMP_INTR_GEQ = 0xB0,
    JMP_INTR_LES = 0xB1,
    JMP_INTR_LEQ = 0xB2,
    JMP_INTR = 0xB3,
    JMP_INTC = 0xB4,

    MEM_READ_INTR_INTC = 0xB5,
    MEM_READ_INTR_INTR = 0xB6,
    MEM_WRITE_INTR_INTC = 0xB7,
    MEM_WRITE_INTR_INTR = 0xB8,


    SETREG_STRR_STRC = 0xC0,
    SETREG_STRR_STRR = 0xC1,
    CCAT_STRR_STRR = 0xC2,
    CCAT_STRR_STRC = 0xC3,
    CMP_STRR_STRR = 0xC4,
    CMP_STRR_STRC = 0xC5,
    STRSPLT_STRR_INTC = 0xC6,
    STRSPLT_STRR_INTR = 0xC7,
    MEM_READ_STRR_INTC = 0xC8,
    MEM_READ_STRR_INTR = 0xC9,
    MEM_WRITE_STRR_INTC = 0xCA,
    MEM_WRITE_STRR_INTR = 0xCB,
    PUSH_STRR = 0xCC,
    PUSH_STRC = 0xCD,
    POP_STRR = 0xCE,


    HALT = 0xFF,
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

public enum IntRegisterIndex
{
    INST_PTR = 0,
    FLAGS_0 = 1,
    RESERVED_0 = 2,
    RESERVED_1 = 3,
    GP_I32_0 = 4,
    GP_I32_1 = 5,
    GP_I32_2 = 6,
    GP_I32_3 = 7,
}

public enum StrRegisterIndex
{
    GP_STR_0 = 0,
    GP_STR_1 = 1,
    GP_STR_2 = 2,
    GP_STR_3 = 3,
}

public readonly struct CpuRegisters
{
    public int INST_PTR => Int[(int)IntRegisterIndex.INST_PTR];
    public int FLAGS_0 => Int[(int)IntRegisterIndex.FLAGS_0];
    public readonly int[] Int;
    public readonly string[] Str;


    public CpuRegisters()
    {
        Int = new int[Enum.GetValues(typeof(IntRegisterIndex)).Length];
        Str = new string[Enum.GetValues(typeof(StrRegisterIndex)).Length];
        for (var i = 0; i < Str.Length; i++) Str[i] = "";
    }

    public int this[IntRegisterIndex key] { get => GetIntRegisterValue(key); set => SetIntRegisterValue(key, value); }

    public string this[StrRegisterIndex key]
    {
        get => GetStrRegisterValue((IntRegisterIndex)key);
        set => SetStrRegisterValue(key, value);
    }

    private void SetStrRegisterValue(StrRegisterIndex key, string value) => Str[(int)key] = value;
    private string GetStrRegisterValue(IntRegisterIndex key) => Str[(int)key];
    public void SetIntRegisterValue(IntRegisterIndex intRegister, int value) => SetIntRegVal((byte)intRegister, value);
    public int GetIntRegisterValue(IntRegisterIndex intRegister) => GetIntRegisterValue((byte)intRegister);
    private void SetIntRegVal(byte register, int value) => Int[register] = value;
    private int GetIntRegisterValue(byte register) => Int[register];


    // @formatter:accessor_owner_body restore
    // @formatter:instance_members_qualify_declared_in restore
    // @formatter:place_expr_accessor_on_single_line restore
    // @formatter:place_simple_accessorholder_on_single_line restore
}