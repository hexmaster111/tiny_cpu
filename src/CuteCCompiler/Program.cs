using System.Collections.Immutable;
using System.Diagnostics;
using TinyAssemblerLib;
using TinyCpuLib;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int globalVarA = 42;
            fn main():void {
                int lv_demo=globalVarA;
            }
            fn other_func ( ) : void { int x=0; }
            main ();
            """;


        ImmutableArray<AsmInst> asm;

        #region Compile

        try
        {
            var words = CuteCWordSplitter.Wordify(input);
            var tokens = CuteCTokenizer.Tokenize(words);
            var rootToken = new ProgramRoot(tokens);
            CuteCLexer.Lex(rootToken);
            var varTable = CuteCVariableTable.MakeTable(rootToken);
            var funcTable = new CuteCFuncTable(rootToken);
            var cuteCAsmTokens = CuteCAsmToken.FromTree(varTable, funcTable, rootToken);
            List<AsmInst> asmOutput = CuteCAsmToken.ConvertToAsm(cuteCAsmTokens);
            asm = asmOutput.ToImmutableArray();

            #endregion

            CuteCVisualisation.DrawCompileSteps(input, words, tokens, rootToken, varTable, cuteCAsmTokens, asmOutput);
        }
        catch (Exception ex)
        {
            throw new Exception("Compiler Error", ex);
        }

        Console.WriteLine("Compiled Ok!");

        #region Assemble

        byte[] finalExe;

        try
        {
            var asmTokens = asm.Select(x => x.AssemblyToken).ToImmutableArray();
            var assembler = new TinyAsmAssembler(asmTokens);
            finalExe = assembler.Assemble();
        }
        catch (Exception ex)
        {
            throw new Exception("Assembler Error", ex);
        }

        Console.WriteLine("Assembled Ok!");

        #endregion


        #region Runtime

        try
        {
            var key = Console.ReadKey();
            bool singleStep = false;
            if (key.Key == ConsoleKey.R) singleStep = false;
            else if (key.Key == ConsoleKey.D) singleStep = true;
            else
            {
                Debugger.Break();
                return;
            }

            TinyRuntime.RuntimeMain(new TinyCpu()
            {
                TCpuExe = finalExe
            }, singleStep);
        }
        catch (Exception ex)
        {
            throw new Exception("Runtime Exception", ex);
        }

        #endregion
    }
}