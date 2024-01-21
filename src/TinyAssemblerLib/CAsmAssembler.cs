using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using TinyAssemblerLib;
using TinyCpuLib;

public static class CAsmAssembler
{
    public static void CAsmAssemblerMain(string[] args)
    {
        // finding casm files
        var assemblyPaths = args
            .Where(File.Exists)
            .Where(x => Path.GetExtension(x) == ".casm")
            .ToArray();

        var asmInputPath = assemblyPaths.FirstOrDefault();
        if (asmInputPath == null)
        {
            Console.WriteLine("No casm file given");
            return;
        }

        //finding where to write the assembly to
        var outputPath = args.FirstOrDefault(x => x.StartsWith("-o"));

        //if we dont find one, default to the name of the casm file with a hec extension
        if (outputPath == null) outputPath = Path.GetFileNameWithoutExtension(asmInputPath) + ".hec";
        else outputPath = outputPath[("-o".Length)..];

        //read the file
        var fileLines = File.ReadAllLines(asmInputPath);
        var hec = AssembleFromFileLines(fileLines);
        WriteAsmToFile(outputPath, hec);
    }

    public static (HecFile file, string verifyLog) AssembleFromFileLinesDbg(string[] fileLines)
    {
        //do build work
        var tokenizer = new TinyAsmTokenizer(fileLines);
        var tokens = tokenizer.Nom();
        return AssembleFromTokens(tokens);
    }

    public static HecFile AssembleFromFileLines(string[] fileLines)
    {
        //do build work
        var tokenizer = new TinyAsmTokenizer(fileLines);
        var tokens = tokenizer.Nom();
        return AssembleFromTokens(tokens).file;
    }

    public static (HecFile file, string verifyLog) AssembleFromTokens(ImmutableArray<TinyAsmTokenizer.Token> tokens)
    {
        var asm = new TinyAsmAssembler(tokens);
        var codeSectionBytes = asm.Assemble();
        var hecFile = HecFile.New(codeSectionBytes);
        return (hecFile, VerifyAssembly(codeSectionBytes, hecFile, asm));
    }

    public static (TinyAsmAssembler, HecFile) AssembleFromTokensToTable(ImmutableArray<TinyAsmTokenizer.Token> tokens)
    {
        var asm = new TinyAsmAssembler(tokens);
        var codeSectionBytes = asm.Assemble();
        var hecFile = HecFile.New(codeSectionBytes);
        VerifyAssembly(codeSectionBytes, hecFile, asm);

        return (asm, hecFile);
    }


    public static void WriteAsmToFile(string outputPath, HecFile hecFile)
    {
        //write the file data to the output file
        File.WriteAllBytes(outputPath, hecFile.GetFileBytes());
    }

    private static string VerifyAssembly(byte[] codeSectionBytes, HecFile hecFile, TinyAsmAssembler asm)
    {
        //Verify the hec file assembles layout is correct to the data we gave it
        var zip = codeSectionBytes.Zip(hecFile.TinyCpuCode);

        bool fail = false;

        Console.WriteLine("Verifying Assembly");
        int byteNumber = 0;
        foreach (var (f, s) in zip)
        {
            if (!fail)
            {
                fail = f != s;
                if (fail) Console.WriteLine($"First mismatch @{byteNumber}");
            }

            if (f != s) Console.ForegroundColor = ConsoleColor.Red;
            Console.Write('.');
            if (f != s) Console.ResetColor();
            byteNumber++;
        }

        if (fail)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Assembly verify failed!");
            throw new Exception("Assembly verify fail");
        }

        Console.WriteLine("Assembly ok");

        var sb = new StringBuilder();
        foreach (var inst in asm.AsmTokens)
        {
            string instString = "";
            var len = 0;
            foreach (var b in inst.GetReadOnlyData())
            {
                var s = "0x" + b.ToString("X2") + ", ";
                instString += s;
                len += s.Length;
            }

            const int commentStart = 40;


            var instAddress = $"{asm.GetInstAddress(inst):x2}:";
            var lp2 = instString;
            var lp3 = $"// [{(OpCode)inst.GetReadOnlyData()[0]}]" +
                      $"{inst.Token.Type}" + $" {inst.Token.ArgumentZeroData}" + $" {inst.Token.ArgumentOneData}";
            sb.Append(instAddress);
            sb.Append(lp2);
            sb.Append(' ', NumGtrThenZero(commentStart - len));
            sb.AppendLine(lp3);
        }

        return sb.ToString();

        int NumGtrThenZero(int i)
        {
            return i > 0 ? i : 0;
        }
    }
}

/// <summary>
///     Just here to remind me that this var may not be needed in the function, and it was passed into another func,
///     that is an auxiliary proc to the first item.
/// </summary>
public class PassThroughAttribute : Attribute
{
}