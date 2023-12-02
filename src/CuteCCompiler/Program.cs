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
            fn main():void {
                int globalVarA = 42;
                
                int lv_demo=globalVarA; // MEM_READ GP_I32_0 0xXX
                                        // MEM_WRITE GP_I32_0 0xXX
                                        
                other_func();
            }
            fn other_func ( ) : void { int x=0; }
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
            var asmOutput = CuteCAsmToken.ConvertToAsm(cuteCAsmTokens);

            //To the top of the asm instructions, we apply our "runtime" code, just call main, and halt 
            var injectedRuntimeInjst = new List<AsmInst>()
            {
                //Call user main
                new(new TinyAsmTokenizer.Token(TinyAsmTokenizer.Token.TokenType.CALL,
                    TinyAsmTokenizer.Token.ArgumentType.STR,
                    TinyAsmTokenizer.Token.ArgumentType.NONE,
                    ".::main")),

                //Halt the cpu
                new(new TinyAsmTokenizer.Token(TinyAsmTokenizer.Token.TokenType.HALT))
            };


            asm = injectedRuntimeInjst.Concat(asmOutput).ToImmutableArray();
            CuteCVisualisation.DrawCompileSteps(
                input, words, tokens, rootToken, varTable, funcTable, cuteCAsmTokens, asmOutput
            );
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Compiler Assembling {ex.Message}");
            Console.ResetColor();
            throw new Exception("Compiler Error", ex);
        }

        #endregion

        Console.WriteLine("Compiled Ok!");

        #region Assemble

        byte[] finalExe;

        try
        {
            var asmTokens = asm.Select(x => x.AssemblyToken).ToImmutableArray();
            var hec = CAsmAssembler.AssembleFromTokens(asmTokens);
            finalExe = hec.TinyCpuCode;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error Assembling {ex.Message}");
            Console.ResetColor();
            throw new Exception("Assembler Error", ex);
        }

        Console.WriteLine("Assembled Ok!");

        #endregion


        #region Runtime

        try
        {
            Console.WriteLine("Press R to run, or D to debug");

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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Runtime Exception: {ex.Message}");
            Console.ResetColor();
            throw new Exception("Runtime Exception", ex);
        }

        #endregion
    }
}