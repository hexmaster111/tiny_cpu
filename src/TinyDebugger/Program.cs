// See https://aka.ms/new-console-template for more information

using Decomp;
using ImGuiNET;
using Raylib_cs;
using rlImGui_cs;
using TinyCpuLib;

namespace TinyDebugger;

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
        List<DecompToken> decomp = new();


        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            rlImGui.Begin();
            TinyCpuUi.Render(ref cpu,ref decomp);
            ImGui.ShowDemoWindow();
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