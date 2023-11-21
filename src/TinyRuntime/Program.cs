// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using TinyAssemblerLib;
using TinyCpuLib;

internal static class Program
{
    public static void Main(string[] args)
    {
        var hecs = args.Where(x => x.EndsWith(".hec")).ToArray();
        var singleStep = args.Any(x => x == "singleStep");

        if (!hecs.Any())
        {
            Console.WriteLine("No files passed in, expected hec file");
            return;
        }

        if (hecs.Length > 1)
        {
            Console.WriteLine("Only one hec at a time please");
            return;
        }

        var file = hecs.First();

        if (!HecFile.TryLoad(file, out var f, out var err))
        {
            Console.WriteLine($"Error loading file {err.Message}");
            Debugger.Break();
            return;
        }


        var tinyCpu = new TinyCpu
        {
            TCpuExe = f.Value.TinyCpuCode
        };

        while (!tinyCpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
        {
            tinyCpu.DumpState();
            if (singleStep)
            {
                WriteWithColoredShortcutKey("STEP", 'S', ConsoleColor.Green);
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.S:
                        tinyCpu.Step();
                        break;
                }
            }
            else
            {
                tinyCpu.Step();
            }
        }

        Console.WriteLine("Program Ended");
    }

    static void WriteWithColoredShortcutKey(string word, char shortcut, ConsoleColor color)
    {
        foreach (var ch in word)
        {
            if (ch == shortcut) Console.ForegroundColor = color;
            Console.Write(ch);
            if (ch == shortcut) Console.ResetColor();
        }
    }
}


//See design.md "FILESYSTEM"