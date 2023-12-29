// See https://aka.ms/new-console-template for more information

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
/*00:*/ 0xA6, // [CALLD] LBL MAIN_LOOP 
/*01:*/ 0xB5, 0x04, 0x03, 0x00, 0x00, 0x00, // [MEM_READ_INTR_INTC] MEM_READ GP_I32_0 3
/*07:*/ 0x0D, 0x04, 0x00, 0x00, 0x00, 0x00, // [CMP_INTR_INTC] CMP GP_I32_0 0
/*0d:*/ 0xA7, 0x28, 0x00, 0x00, 0x00, // [JMP_INTC_EQ] JMP_EQ START_FILL 
/*12:*/ 0xB5, 0x04, 0x02, 0x00, 0x00, 0x00, // [MEM_READ_INTR_INTC] MEM_READ GP_I32_0 2
/*18:*/ 0x0D, 0x04, 0x01, 0x00, 0x00, 0x00, // [CMP_INTR_INTC] CMP GP_I32_0 1
/*1e:*/ 0xA7, 0x3A, 0x00, 0x00, 0x00, // [JMP_INTC_EQ] JMP_EQ STOP_FILL 
/*23:*/ 0xB4, 0x00, 0x00, 0x00, 0x00, // [JMP_INTC] JMP MAIN_LOOP 
/*28:*/ 0xA6, // [CALLD] LBL START_FILL 
/*29:*/ 0x01, 0x06, 0x01, 0x00, 0x00, 0x00, // [SETREG_INTR_INTC] SETREG GP_I32_2 1
/*2f:*/ 0xB7, 0x06, 0x09, 0x00, 0x00, 0x00, // [MEM_WRITE_INTR_INTC] MEM_WRITE GP_I32_2 9
/*35:*/ 0xB4, 0x00, 0x00, 0x00, 0x00, // [JMP_INTC] JMP MAIN_LOOP 
/*3a:*/ 0xA6, // [CALLD] LBL STOP_FILL 
/*3b:*/ 0x01, 0x06, 0x00, 0x00, 0x00, 0x00, // [SETREG_INTR_INTC] SETREG GP_I32_2 0
/*41:*/ 0xB7, 0x06, 0x09, 0x00, 0x00, 0x00, // [MEM_WRITE_INTR_INTC] MEM_WRITE GP_I32_2 9
/*47:*/ 0xB4, 0x00, 0x00, 0x00, 0x00, // [JMP_INTC] JMP MAIN_LOOP 
        };
        cpu.LoadProgram(program);
        var decomp = new TinyAsmDecompiler().Decompile(program);


        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            rlImGui.Begin();

            //Set Font Size
            //Funny fontsize hack fond here for fontsize issues:
            //https://github.com/ocornut/imgui/issues/1018
            var oldSize = ImGui.GetFont().Scale;
            ImGui.GetFont().Scale *= 2f;
            ImGui.PushFont(ImGui.GetFont());
            Ui.Render();
            ImGui.GetFont().Scale = oldSize;
            ImGui.PopFont();

            ImGui.End();
            rlImGui.End();
            Raylib.EndDrawing();
        }

        rlImGui.Shutdown();
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