using System.Collections.Immutable;
using System.Runtime.InteropServices;
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


    public static HecFile AssembleFromFileLines(string[] fileLines)
    {
        //do build work
        var tokenizer = new TinyAsmTokenizer(fileLines);
        var tokens = tokenizer.Nom();
        return AssembleFromTokens(tokens);
    }

    public static HecFile AssembleFromTokens(ImmutableArray<TinyAsmTokenizer.Token> tokens)
    {
        var asm = new TinyAsmAssembler(tokens);
        var codeSectionBytes = asm.Assemble();
        var hecFile = HecFile.New(codeSectionBytes);
        VerifyAssembly(codeSectionBytes, hecFile, asm);

        return hecFile;
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


    private static void VerifyAssembly(byte[] codeSectionBytes, HecFile hecFile, TinyAsmAssembler asm)
    {
        //Verify the hec file assembles layout is correct to the data we gave it
        var zip = codeSectionBytes.Zip(hecFile.TinyCpuCode);
#if DEBUG
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

        foreach (var inst in asm.AsmTokens)
        {
            string instString = "";
            foreach (var b in inst.GetReadOnlyData())
            {
                instString += "0x" + b.ToString("X2") + ", ";
            }

            Console.WriteLine($"/*{asm.GetInstAddress(inst):x2}:*/ " +
                              instString +
                              $"// [{(OpCode)inst.GetReadOnlyData()[0]}] {inst.Token.Type} {inst.Token.ArgumentZeroData} {inst.Token.ArgumentOneData}");
        }
#endif
    }
}

/// <summary>
///     Just here to remind me that this var may not be needed in the function, and it was passed into another func,
///     that is an auxiliary proc to the first item.
/// </summary>
public class PassThroughAttribute : Attribute
{
}