using System.Numerics;
using ImGuiNET;
using TinyCpuLib;

namespace TinyDebugger;

internal static class TinyByteCodeIde
{
    private static string _currentWorkingBuffer = "";


    public static void Render(ref TinyCpu cpu)
    {
        ImGui.Begin("TinyByteCodeIde");
        {
            if (ImGui.Button("Read CPU into Buffer"))
            {
                try
                {
                    var decompTokens = new Decomp.TinyAsmDecompiler().Decompile(cpu.TCpuExe);
                    _currentWorkingBuffer += ";; CPU EXE -- start\n";
                    foreach (var token in decompTokens)
                    {
                        _currentWorkingBuffer += token.data.Aggregate("", (current, b) => current + $"{b:X2} ");
                        _currentWorkingBuffer += "\n";
                    }

                    _currentWorkingBuffer += ";; CPU EXE -- end\n";
                }
                catch (Exception e)
                {
                    _currentWorkingBuffer = e.Message;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Write Buffer into CPU"))
            {
                try
                {
                    List<byte> bytes = new();
                    foreach (var line in _currentWorkingBuffer.Split('\n'))
                    {
                        if (line.StartsWith(";")) continue;
                        var tokens = line.Split(' ');
                        foreach (var token in tokens)
                        {
                            if (token == "" || token.StartsWith(";")) continue;
                            bytes.Add(byte.Parse(token, System.Globalization.NumberStyles.HexNumber));
                        }
                    }

                    cpu.TCpuExe = bytes.ToArray();
                }
                catch (Exception e)
                {
                    //ignore... shouldnt tho
                }
            }


            ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]);
            ImGui.SetWindowFontScale(2);

            try
            {
                ImGui.InputTextMultiline("##byteCodefileText", ref _currentWorkingBuffer, 100000,
                    new Vector2(ImGui.GetWindowWidth() - 20, ImGui.GetWindowHeight() - 75));
            }
            catch (Exception e)
            {
                //ignore, throws when copy pasting... works right tho
            }

            ImGui.PopFont();
            ImGui.SetWindowFontScale(1);
        }
        ImGui.End();
    }
}