// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using TinyAssemblerLib;
using TinyCpuLib;

public static class Program
{
    public static void Main(string[] args) => TinyRuntime.RuntimeMain(args);
}

public static class TinyRuntime
{
    public static void RuntimeMain(string[] args)
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
        RuntimeMain(file, singleStep);
    }

    public static void RuntimeMain(string filePath, bool singleStep)
    {
        if (!HecFile.TryLoad(filePath, out var f, out var err))
        {
            Console.WriteLine($"Error loading file {err.Message}");
            Debugger.Break();
            return;
        }

        RuntimeMain(f.Value, singleStep);
    }

    public static void RuntimeMain(HecFile f, bool singleStep)
    {
        var tinyCpu = new TinyCpu
        {
            TCpuExe = f.TinyCpuCode
        };

        RuntimeMain(tinyCpu, singleStep);
    }

    public static void RuntimeMain(TinyCpu tinyCpu, bool singleStep, Action<TinyCpu>? dumpState = null)
    {
        while (!tinyCpu.Reg.FLAGS_0.ReadBit((int)FLAGS_0_USAGE.HALT))
        {
            if (dumpState == null)
            {
                var s = TinyCpuVisualisation.DumpState(tinyCpu);
                Console.Clear();
                Console.WriteLine(s);
            }
            else dumpState(tinyCpu);

            if (singleStep)
            {
                WriteWithColoredShortcutKey("STEP", 'S', ConsoleColor.Green);
                WriteWithColoredShortcutKey(" CONTINUE", 'C', ConsoleColor.Green);
                Console.WriteLine();
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.S:
                        tinyCpu.Step();
                        break;
                    case ConsoleKey.C:
                        singleStep = false;
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