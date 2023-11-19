// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TinyCpuLib;

internal static class Program
{
    public static void Main(string[] args)
    {
        // var fileData = new byte[]
        // {
        //     0xFA, 0xDD, 0xED, 0xD0, 0x67, 0x00, 0x00, 0x00,
        //     0x0C, 0x00, 0x00, 0x00, //CODE_END_OFFSET 12
        //     //CODE SECTION START
        //     /*CODE::00:*/ 0x01, 0x05, 0x01, 0x00, 0x00, 0x00, // [SETREG_R_C] SETREG GP_I32_1 1
        //     /*CODE::06:*/ 0xB7, 0x05, 0x01, 0x00, 0x00, 0x00, // [MEM_WRITE_R_C] MEM_WRITE GP_I32_1 1
        //     /*CODE::0c:*/ 0xB8, 0x05, 0x04, // [MEM_WRITE_R_R] MEM_WRITE GP_I32_1 GP_I32_0
        //     /*CODE::0f:*/ 0xB5, 0x05, 0x01, 0x00, 0x00, 0x00, // [MEM_READ_R_C] MEM_READ GP_I32_1 1
        //     /*CODE::15:*/ 0xFF, // [HALT] HALT 
        // };

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
public readonly struct HecFile
{
    private readonly byte[] _fileBytes;
    private HecFile(byte[] fileBytes) => _fileBytes = fileBytes;
    public byte[] MagicNumber => _fileBytes[..8];
    public byte[] CodeEndOffsetBytes => _fileBytes[8..12];
    public int CODE_END_OFFSET => BitConverter.ToInt32(CodeEndOffsetBytes);
    public byte[] TinyCpuCode => _fileBytes[CODE_END_OFFSET..];


    public static bool TryLoad(
        string path,
        [NotNullWhen(true)] out HecFile? file,
        [NotNullWhen(false)] out HecFileLoadException? loadException
    )
    {
        try
        {
            file = Load(path);
            loadException = null;
            return true;
        }
        catch (HecFileLoadException ex)
        {
            file = null;
            loadException = ex;
            return false;
        }
    }


    public static HecFile Load(byte[] fileBytes)
    {
        var hecFile = new HecFile(fileBytes);
        Validate(hecFile); //Validate will throw LoadExcpetions 
        return hecFile;
    }

    public static HecFile Load(string path) => Load(File.ReadAllBytes(path));

    private static void Validate(HecFile f)
    {
        try
        {
            var a = f.MagicNumber;
            if (!a.SequenceEqual(MAGIC_NUMBER)) throw new HecFileLoadException();
        }
        catch
        {
            throw new HecFileLoadException("INVALID FILE TYPE");
        }
    }

    private static readonly byte[] MAGIC_NUMBER = { 0xFA, 0xDD, 0xED, 0xD0, 0x67, 0x00, 0x00, 0x00 };

    public class HecFileLoadException : Exception
    {
        public HecFileLoadException(string message) : base(message)
        {
        }

        public HecFileLoadException()
        {
        }
    }
}