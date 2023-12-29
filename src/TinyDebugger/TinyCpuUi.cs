using System.Numerics;
using Decomp;
using ImGuiNET;
using Raylib_cs;
using TinyCpuLib;

namespace TinyDebugger;

internal static partial class TinyCpuUi
{
    public static bool RunCpu = false;
    private static SimpleTimer _runTimer = new(1000);

    private static bool _drawRegistersWindow = true;
    private static bool _drawMemoryWindow = true;
    private static bool _drawDumpWindow = false;
    private static bool _drawDecompWindow = true;
    private static bool _drawReferenceWindow = false;
    private static bool _drawControlMenu = true;
    private static bool _drawTankFillExample = false;


    public static void Render(TinyCpu cpu, List<DecompToken> decomp)
    {
        ImGui.DockSpaceOverViewport();
        DrawMainMenuBar();
        if (_drawRegistersWindow) DrawRegistersWindow(cpu);
        if (_drawMemoryWindow) DrawMemoryWindow(cpu);
        if (_drawDumpWindow) DrawDumpWindow(cpu);
        if (_drawDecompWindow) DrawDecompWindow(decomp, cpu.Reg.INST_PTR);
        if (_drawControlMenu) DrawCpuControlWindow(cpu);
        if (_drawTankFillExample) DrawTankFillExample(cpu.Memory);


        var highlightOpcode = OpCode.NOOP;
        if (cpu.Reg.INST_PTR >= 0 && cpu.Reg.INST_PTR < cpu.TCpuExe.Length)
            highlightOpcode = (OpCode)cpu.TCpuExe[cpu.Reg.INST_PTR];
        if (_drawReferenceWindow) DrawReferenceWindow(highlightOpcode);

        ImGui.End();

        if (RunCpu)
        {
            if (!_runTimer.Evaluate(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)) return;
            if (!cpu.Reg.FLAGS_0_HALT) cpu.Step();
            else RunCpu = false;
        }
    }

    private static float _tankFillExample_tankLevel = 0.00f;
    private static bool _tankFillExample_enable = false;

    private static void DrawTankFillExample(IMemory cpuMemory)
    {
        if (!ImGui.Begin("TinyCPU Fill Seneraro", ref _drawTankFillExample)) return;
        ImGui.Checkbox("Enable", ref _tankFillExample_enable);


        //Tank logic
        if (_tankFillExample_enable)
        {
            const float lowTankFs = 0.25f;
            const float highTankFs = 0.75f;
            const float tankDrainRate = 0.002f;
            const float tankFillRate = tankDrainRate + .001f;

            //TODO: Sim controlled var
            var isDraning = true;

            var lowFs = _tankFillExample_tankLevel < lowTankFs;
            var highFs = _tankFillExample_tankLevel > highTankFs;
            var isFilling = cpuMemory[9] == 1;

            var isEmpty = _tankFillExample_tankLevel <= 0f;
            var isFull = _tankFillExample_tankLevel >= 1f;

            if (isDraning) _tankFillExample_tankLevel -= tankDrainRate;
            if (isFilling) _tankFillExample_tankLevel += tankFillRate;
            _tankFillExample_tankLevel = Math.Clamp(_tankFillExample_tankLevel, 0f, 1f);

            cpuMemory[2] = highFs ? 1 : 0;
            cpuMemory[3] = lowFs ? 0 : 1;
        }


        if (ImGui.SliderFloat("fillTankSlider", ref _tankFillExample_tankLevel, 0f, 1f)) ;
        ImGui.Text($"High FS = {cpuMemory[2]} Low FS = {cpuMemory[3]} Fill = {cpuMemory[9]}");
        ImGui.End();
    }

    private static void DrawMainMenuBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Exit")) Raylib.CloseWindow();
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                ImGui.MenuItem("Registers", "", ref _drawRegistersWindow);
                ImGui.MenuItem("Memory", "", ref _drawMemoryWindow);
                ImGui.MenuItem("Dump", "", ref _drawDumpWindow);
                ImGui.MenuItem("Decomp", "", ref _drawDecompWindow);
                ImGui.MenuItem("Reference", "", ref _drawReferenceWindow);
                ImGui.MenuItem("Control", "", ref _drawControlMenu);
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Examples"))
            {
                ImGui.MenuItem("Tank Fill", "", ref _drawTankFillExample);
                ImGui.EndMenu();
            }
        }

        ImGui.EndMainMenuBar();
    }

    private static void DrawCpuControlWindow(TinyCpu cpu)
    {
        ImGui.Begin("TinyCpu -- Control", ref _drawControlMenu);

        if (ImGui.Button("STEP"))
        {
            cpu.Step();
        }

        ImGui.SameLine();

        if (ImGui.Button(!RunCpu ? "RUN" : "PAUSE"))
        {
            RunCpu = !RunCpu;
        }


        ImGui.Separator();
        ImGui.Columns(2);
        ImGui.Text("Cycle Time (Hz)");

        var cpuCycleTimeHz = cpu.CycleTimeHz;
        ImGui.InputInt("##CycleTimeHz", ref cpuCycleTimeHz);
        if (cpuCycleTimeHz == 0) cpuCycleTimeHz = 10;
        if (cpu.CycleTimeHz != cpuCycleTimeHz) _runTimer = new SimpleTimer(1000 / cpuCycleTimeHz);
        cpu.CycleTimeHz = cpuCycleTimeHz;
        ImGui.NextColumn();
        ImGui.Text("Cycle Time (ms)");
        ImGui.Text($"{1000f / cpu.CycleTimeHz:F2}");
        ImGui.NextColumn();
        ImGui.Columns(1);


        ImGui.End();
    }

    private static void DrawReferenceWindow(OpCode highlightOpcode)
    {
        ImGui.Begin("TinyCpu -- Reference");


        ImGui.Columns(2);
        ImGui.Text("OpCode");
        ImGui.NextColumn();
        ImGui.Text("Description");
        ImGui.Separator();
        ImGui.NextColumn();


        foreach (var VARIABLE in Enum.GetValues(typeof(OpCode)))
        {
            var opCode = (OpCode)VARIABLE;
            var opCodeName = opCode.ToString();
            var opCodeDesc = GetOpCodeDescription(opCode);
            var opCodeColor = new Vector4(1, 1, 1, 1);
            if (opCode == highlightOpcode) opCodeColor = new Vector4(1, 0, 0, 1);
            ImGui.PushStyleColor(ImGuiCol.Text, opCodeColor);
            ImGui.Text(opCodeName);
            ImGui.NextColumn();
            ImGui.Text(opCodeDesc);
            ImGui.NextColumn();
            ImGui.PopStyleColor();
        }

        ImGui.End();
    }


    private static string GetOpCodeDescription(OpCode opCode) => opCode switch
    {
        OpCode.NOOP => "TODO: My Desc",
        OpCode.SETREG_R_C => "TODO: My Desc",
        OpCode.SETREG_R_R => "TODO: My Desc",
        OpCode.ADD_R_C => "TODO: My Desc",
        OpCode.ADD_R_R => "TODO: My Desc",
        OpCode.MUL_R_C => "TODO: My Desc",
        OpCode.MUL_R_R => "TODO: My Desc",
        OpCode.SUB_R_C => "TODO: My Desc",
        OpCode.SUB_R_R => "TODO: My Desc",
        OpCode.DIV_R_C => "TODO: My Desc",
        OpCode.DIV_R_R => "TODO: My Desc",
        OpCode.INC => "TODO: My Desc",
        OpCode.DEC => "TODO: My Desc",
        OpCode.CMP_R_C => "TODO: My Desc",
        OpCode.CMP_R_R => "TODO: My Desc",
        OpCode.PUSH_C => "TODO: My Desc",
        OpCode.PUSH_R => "TODO: My Desc",
        OpCode.POP_R => "TODO: My Desc",
        OpCode.CALL_C => "TODO: My Desc",
        OpCode.CALL_R => "TODO: My Desc",
        OpCode.RET => "TODO: My Desc",
        OpCode.CALL_D => "TODO: My Desc",
        OpCode.JMP_C_EQ => "TODO: My Desc",
        OpCode.JMP_C_NEQ => "TODO: My Desc",
        OpCode.JMP_C_GTR => "TODO: My Desc",
        OpCode.JMP_C_GEQ => "TODO: My Desc",
        OpCode.JMP_C_LES => "TODO: My Desc",
        OpCode.JMP_C_LEQ => "TODO: My Desc",
        OpCode.JMP_R_EQ => "TODO: My Desc",
        OpCode.JMP_R_NEQ => "TODO: My Desc",
        OpCode.JMP_R_GTR => "TODO: My Desc",
        OpCode.JMP_R_GEQ => "TODO: My Desc",
        OpCode.JMP_R_LES => "TODO: My Desc",
        OpCode.JMP_R_LEQ => "TODO: My Desc",
        OpCode.JMP_R => "TODO: My Desc",
        OpCode.JMP_C => "TODO: My Desc",
        OpCode.MEM_READ_R_C => "TODO: My Desc",
        OpCode.MEM_READ_R_R => "TODO: My Desc",
        OpCode.MEM_WRITE_R_C => "TODO: My Desc",
        OpCode.MEM_WRITE_R_R => "TODO: My Desc",
        OpCode.HALT => "TODO: My Desc",
        _ => "Im new exciting and undocumented!"
    };


    private static void DrawDecompWindow(List<DecompToken> decomp, int instPtr)
    {
        ImGui.Begin("TinyCpu -- Decomp", ref _drawDecompWindow);
        ImGui.Columns(4);
        ImGui.Text("Address");
        ImGui.NextColumn();
        ImGui.Text("OpCode");
        ImGui.NextColumn();
        ImGui.Text("Arg0");
        ImGui.NextColumn();
        ImGui.Text("Arg1");
        ImGui.NextColumn();

        foreach (var dt in decomp)
        {
            if (instPtr == dt.address) ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            ImGui.Text($"{dt.address:X4}");
            ImGui.NextColumn();
            ImGui.Text(dt.opCode.ToString());
            ImGui.NextColumn();
            ImGui.Text(dt.sarg0);
            ImGui.NextColumn();
            ImGui.Text(dt.sarg1);
            ImGui.NextColumn();
            if (instPtr == dt.address) ImGui.PopStyleColor();
        }

        ImGui.End();
    }

    private static void DrawDumpWindow(TinyCpu cpu)
    {
        if (ImGui.Begin("TinyCpu -- Dump"))
        {
            try
            {
                ImGui.Text(TinyCpuVisualisation.DumpState(cpu));
            }
            catch (Exception e)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
                ImGui.Text(e.ToString());
                ImGui.PopStyleColor();
            }
        }

        ImGui.End();
    }


    private static void DrawRegistersWindow(TinyCpu cpu)
    {
        if (ImGui.Begin("TinyCpu - Registers", ref _drawRegistersWindow))
        {
            // draw a table of registers 
            // | Register | Value (DEC)| Value (HEX) |
            // |----------|------------|-------------|
            // | GP_I32_0 | 0          | 0x00000000  |

            ImGui.Columns(3);
            ImGui.Text("Register");
            ImGui.NextColumn();
            ImGui.Text("Value (DEC)");
            ImGui.NextColumn();
            ImGui.Text("Value (HEX)");
            ImGui.Separator();
            ImGui.NextColumn();

            var enumNames = Enum.GetNames(typeof(RegisterIndex));

            for (var i = 0; i < cpu.Reg.Data.Length; i++)
            {
                ref var regVal = ref cpu.Reg.Data[i];
                var regName = enumNames[i];
                ImGui.Text(regName);
                ImGui.NextColumn();
                ImGui.InputInt($"##{regName}", ref regVal);
                ImGui.NextColumn();
                ImGui.Text(regVal.ToString("X8"));
                ImGui.NextColumn();
            }

            ImGui.Columns(1);
            ImGui.Separator();
        }

        ImGui.End();
    }

    private static void DrawMemoryWindow(TinyCpu cpu)
    {
        if (ImGui.Begin("TinyCpu - Memory", ref _drawMemoryWindow))
        {
            // draw a table of memory
            // | Address | Value (DEC)| Value (HEX) |
            // |----------|------------|-------------|
            // | 0x00000000 | 0          | 0x00000000  |
            ImGui.Columns(3);
            ImGui.Text("Address");
            ImGui.NextColumn();
            ImGui.Text("Value (DEC)");
            ImGui.NextColumn();
            ImGui.Text("Value (HEX)");
            ImGui.Separator();
            ImGui.NextColumn();

            for (var i = 0; i < cpu.Memory.MemorySize; i++)
            {
                var memValTmp = cpu.Memory[i];
                var memName = i.ToString("X4");
                ImGui.Text(memName);
                ImGui.NextColumn();
                ImGui.InputInt($"##{memName}", ref memValTmp);
                ImGui.NextColumn();
                ImGui.Text(memValTmp.ToString("X8"));
                ImGui.NextColumn();

                cpu.Memory[i] = memValTmp;
            }

            ImGui.Columns(1);
            ImGui.Separator();
        }

        ImGui.End();
    }
}