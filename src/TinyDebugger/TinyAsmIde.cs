using System.Numerics;
using Decomp;
using ImGuiNET;
using TinyCpuLib;

namespace TinyDebugger;

public static class TinyAsmIde
{
    public static string FileText =
        """
        ;; IN  HHigh - MEM 0
        ;; IN  LLow - MEM 1
        ;; OUT FillFast - Mem 8



        ; IN  HIGH - MEM 2
        ; IN  LOW  - MEM 3
        ; OUT FILL - Mem 9


        LBL MAIN_LOOP           ; main loop for the application
            MEM_READ GP_I32_0 0x03  ; read INPUT LOW INTO GP_I32_0
            CMP GP_I32_0 0x00       ; comapre input low and the const 0x00
            
            ;if the low switch is off, we need to fill
            ;if the high switch is on, stop fill
            
            JMP_EQ START_FILL       ; Low fs was off, out of water, start fill.
            MEM_READ GP_I32_0 0x02  ; read high fs into GP_I32_0
            CMP GP_I32_0 0x01       ; compare to on
            JMP_EQ STOP_FILL        ; high level fs is on, stop fill
        JMP MAIN_LOOP
            
            
        LBL START_FILL
            SETREG GP_I32_2 0x01       ; Load a 1 into a reg
            MEM_WRITE GP_I32_2 0x09 ; open the fill valve
        JMP MAIN_LOOP
            
        LBL STOP_FILL
            SETREG GP_I32_2 0x00       ; Load a 0 into a reg
            MEM_WRITE GP_I32_2 0x09 ; shut the fill valve
        JMP MAIN_LOOP
        """;

    private static bool _drawIdeWindow = true;
    private static string _lastVerifyLog = "";


    public static void Render(ref TinyCpu cpu, ref List<DecompToken> decomp)
    {
        if (_drawIdeWindow) DrawIdeWindow(ref cpu, ref decomp);
    }

    private static void DrawIdeWindow(ref TinyCpu cpu, ref List<DecompToken> decomp)
    {
        if (ImGui.Begin("TinyAsmIde", ref _drawIdeWindow))
        {
            if (ImGui.Button("Assemble! "))
            {
                try
                {
                    cpu = new TinyCpu();
                    var hec = CAsmAssembler.AssembleFromFileLinesDbg(
                        FileText.Split(new char[] { '\r', '\n' },
                            StringSplitOptions.RemoveEmptyEntries |
                            StringSplitOptions.TrimEntries)
                    );
                    _lastVerifyLog = hec.verifyLog;
                    byte[] program = hec.file.TinyCpuCode;
                    cpu.LoadProgram(program);


                    decomp = new TinyAsmDecompiler().Decompile(program);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Set font size to 2x 
            ImGui.PushFont(ImGui.GetIO().Fonts.Fonts[0]);
            ImGui.SetWindowFontScale(2);

            try
            {
                ImGui.InputTextMultiline("##fileText", ref FileText, 100000,
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

        if (ImGui.Begin("TinyIde Verify Log", ref _drawIdeWindow))
        {
            ImGui.Text(_lastVerifyLog);
        }

        ImGui.End();
    }
}