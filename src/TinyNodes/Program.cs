// See https://aka.ms/new-console-template for more information

using System.Numerics;
using System.Reflection;
using Decomp;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using TinyAssemblerLib;
using TinyCpuLib;

namespace TinyNodes;

internal static class Program
{
    public static void Main(string[] args)
    {
        Raylib.InitWindow(800, 450, "TinyNodes");
        Raylib.SetTargetFPS(60);
        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_WINDOW_MAXIMIZED);
        Raylib.SetExitKey(KeyboardKey.KEY_SCROLL_LOCK);

        rlImGui.Setup(enableDocking: true, darkTheme: true);


        var cpu = new TinyCpu();
        byte[] program =
        {
/*00:*/ 0xA6, // [CALL_D] LBL START 
/*01:*/ 0x0B, 0x04, // [INC] INC GP_I32_0 
/*03:*/ 0x0D, 0x04, 0xFE, 0x00, 0x00, 0x00, // [CMP_R_C] CMP GP_I32_0 FE
/*09:*/ 0xA9, 0x13, 0x00, 0x00, 0x00, // [JMP_C_GTR] JMP_GTR DONE 
/*0e:*/ 0xB4, 0x00, 0x00, 0x00, 0x00, // [JMP_C] JMP START 
/*13:*/ 0xA6, // [CALL_D] LBL DONE 
/*14:*/ 0x0B, 0x05, // [INC] INC GP_I32_1 
/*16:*/ 0xFF, // [HALT] HALT 
        };
        cpu.LoadProgram(program);
        var decomp = new TinyAsmDecompiler().Decompile(program);


        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            rlImGui.Begin();
            // Ui.Render();
            TinyCpuUi.Render(cpu, decomp);

            rlImGui.End();
            Raylib.EndDrawing();
        }

        rlImGui.Shutdown();
    }
}

internal static class TinyCpuUi
{
    public static bool RunCpu = false;
    private static SimpleTimer _runTimer = new(1000);

    public static void Render(TinyCpu cpu, List<DecompToken> decomp)
    {
        ImGui.DockSpaceOverViewport();
        DrawRegistersWindow(cpu);
        DrawMemoryWindow(cpu);
        DrawDumpWindow(cpu);
        DrawCpuControlWindow(cpu);
        DrawDecompWindow(decomp, cpu.Reg.INST_PTR);

        var highlightOpcode = OpCode.NOOP;
        if (cpu.Reg.INST_PTR >= 0 && cpu.Reg.INST_PTR < cpu.TCpuExe.Length)
            highlightOpcode = (OpCode)cpu.TCpuExe[cpu.Reg.INST_PTR];
        DrawReferenceWindow(highlightOpcode);

        ImGui.ShowDemoWindow();
        ImGui.End();

        if (RunCpu)
        {
            if (!_runTimer.Evaluate(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond)) return;
            if (!cpu.Reg.FLAGS_0_HALT) cpu.Step();
            else RunCpu = false;
        }
    }

    private static void DrawCpuControlWindow(TinyCpu cpu)
    {
        ImGui.Begin("TinyCpu -- Control");

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
        if(cpuCycleTimeHz == 0) cpuCycleTimeHz = 1;
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
        ImGui.Begin("TinyCpu -- Decomp");
        ImGui.Columns(6);
        ImGui.Text("Address");
        ImGui.NextColumn();
        ImGui.Text("OpCode");
        ImGui.NextColumn();
        ImGui.Text("Arg0");
        ImGui.NextColumn();
        ImGui.Text("Arg0 Type");
        ImGui.NextColumn();
        ImGui.Text("Arg1");
        ImGui.NextColumn();
        ImGui.Text("Arg1 Type");
        ImGui.Separator();

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
            ImGui.Text(dt.argZeroType.ToString());
            ImGui.NextColumn();
            ImGui.Text(dt.sarg1);
            ImGui.NextColumn();
            ImGui.Text(dt.argOneType.ToString());
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
        if (ImGui.Begin("TinyCpu - Registers"))
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
        if (ImGui.Begin("TinyCpu - Memory"))
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
                var memName = i.ToString("X8");
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

public class SimpleTimer
{
    private readonly long _delay;
    private long _lastFireTime;
    private long _fireRate;

    public SimpleTimer(long delay)
    {
        _delay = delay;
    }

    public long SleepTimeMs { get; private set; }

    public bool Evaluate(long now)
    {
        if (now - _lastFireTime < _fireRate)
        {
            SleepTimeMs = _fireRate - (now - _lastFireTime);
            return false;
        }

        _lastFireTime = now;
        _fireRate = _delay;
        return true;
    }
}