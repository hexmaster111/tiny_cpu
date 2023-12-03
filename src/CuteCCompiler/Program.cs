using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Spectre.Console;
using TinyAssemblerLib;
using TinyCpuLib;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int globalVarA;
            fn main():void {
                globalVarA = 42;
                int other = 420;
                int lv_demo=globalVarA;
                other_func();
            }
            fn other_func ( ) : void {
                int x=20;
                int y;
            }
            """;


        ImmutableArray<AsmInst> asm;
        CuteCFuncTable funcTable;
        CuteCVariableTable varTable;

        #region Compile

        try
        {
            var words = CuteCWordSplitter.Wordify(input);
            var tokens = CuteCTokenizer.Tokenize(words);
            var rootToken = new ProgramRoot(tokens);
            CuteCLexer.Lex(rootToken);
            varTable = CuteCVariableTable.MakeTable(rootToken);
            funcTable = new CuteCFuncTable(rootToken);
            var cuteCAsmTokens = CuteCAsmToken.FromTree(varTable, funcTable, rootToken);
            var asmOutput = CuteCAsmToken.ConvertToAsm(cuteCAsmTokens);
            //To the top of the asm instructions, we apply our "runtime" code, just call main, and halt 
            var injectedRuntimeInst = new List<AsmInst>()
            {
                //Call user main
                new(new TinyAsmTokenizer.Token(TinyAsmTokenizer.Token.TokenType.CALL,
                    TinyAsmTokenizer.Token.ArgumentType.STR,
                    TinyAsmTokenizer.Token.ArgumentType.NONE,
                    ".::main")),

                //Halt the cpu
                new(new TinyAsmTokenizer.Token(TinyAsmTokenizer.Token.TokenType.HALT))
            };


            asm = injectedRuntimeInst.Concat(asmOutput).ToImmutableArray();
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

        (TinyAsmAssembler assembler, HecFile hecFile) hec;

        byte[] finalExe;

        try
        {
            var asmTokens = asm.Select(x => x.AssemblyToken).ToImmutableArray();
            hec = CAsmAssembler.AssembleFromTokensToTable(asmTokens);
            finalExe = hec.hecFile.TinyCpuCode;
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
            Console.WriteLine("Press R to run, or S to single step");

            var key = Console.ReadKey();
            bool singleStep = false;
            if (key.Key == ConsoleKey.R) singleStep = false;
            else if (key.Key == ConsoleKey.S) singleStep = true;
            else
            {
                Debugger.Break();
                return;
            }

            TinyRuntime.RuntimeMain(new TinyCpu()
            {
                TCpuExe = finalExe,
            }, singleStep, c => DumpStateRich(c, funcTable, varTable, hec.assembler));
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


    public static void DumpStateRich(TinyCpu cpu, CuteCFuncTable ft, CuteCVariableTable vt, TinyAsmAssembler asm)
    {
        var curr = "";

        foreach (var inst in asm.AsmTokens)
        {
            if (cpu.Reg.INST_PTR != asm.GetInstAddress(inst)) continue;
            curr = $"{inst.Token.Type} {inst.Token.ArgumentZeroData} {inst.Token.ArgumentOneData}";
        }


        var cpuRegisterTable = new Table().AddColumns("REGISTER", "DEC", "HEX");
        for (int i = 0; i < (int)RegisterIndex.__REGISTER__COUNT__; i++)
        {
            var reg = (RegisterIndex)i;
            var val = cpu.Reg[reg];
            cpuRegisterTable.AddRow(
                new Text(reg.ToString()),
                new Text(val.ToString()),
                new Text(val.ToString("X4")));
        }


        var fnTable = new Table();
        fnTable.AddColumns("NAMESPACE", "FN NAME");
        foreach (var kvp in ft.FuncDictionary)
        {
            foreach (var funcDef in kvp.Value)
            {
                var fnName = funcDef.Key;
                fnTable.AddRow(new Markup(kvp.Key), new Markup(fnName));
            }
        }

        var varTbl = new Table();
        varTbl.AddColumns("Var Slot", "Fullname", "Value");
        var dbgMem = cpu.Memory.Debugger_ReadAllMemoryAddresses();
        foreach (var kvp in vt.VarTable)
        {
            var val = dbgMem[kvp.Value];
            varTbl.AddRow(new Markup(kvp.Value.ToString("000")),
                new Markup(kvp.Key), new Markup(val.ToString()));
        }


        var pgmTbl = new Table()
            .AddColumns(
                "BS",
                "I",
                "OP",
                "ARG 0",
                "ARG 1",
                "INST BYTES"
            );
        foreach (var inst in asm.AsmTokens)
        {
            var instString = "";
            foreach (var b in inst.GetReadOnlyData())
            {
                instString += "0x" + b.ToString("X2") + ", ";
            }

            var pcMark = cpu.Reg.INST_PTR != asm.GetInstAddress(inst) ? string.Empty : "[yellow]>[/]";

            pgmTbl.AddRow(
                new Text($"{asm.GetInstAddress(inst):x2}:"),
                new Markup(pcMark),
                new Text(((OpCode)inst.GetReadOnlyData()[0]).ToString()),
                new Text(inst.Token.ArgumentZeroData),
                new Text(inst.Token.ArgumentOneData),
                new Text(instString)
            );
        }

        var callstackCount = cpu.CallStack.Count;
        var callStackTbl = new Table().AddColumns($"CALL STACK - {callstackCount}");
        foreach (var nextAddr in cpu.CallStack.ToArray())
        {
            callStackTbl.AddRow(new Text(nextAddr.ToString()));
        }


        var varStackTbl = new Table().AddColumns("Value");
        foreach (var value in cpu.ValueStack.ToArray())
        {
            varStackTbl.AddRow(new Text(value.ToString()));
        }


        var cpuTable = new Table().AddColumns($"PROGRAM |{curr}", "Cute C Debug");
        var dbgTbl = new Grid().AddColumns(new GridColumn())
            .AddRow(cpuRegisterTable)
            .AddRow(varTbl)
            .AddRow(fnTable)
            //"Call Stack", "Var Stack").AddRow(callStackTbl, varStackTbl));
            .AddRow(new Grid().AddColumns(new GridColumn(), new GridColumn())
                .AddRow("Call Stack", "Var Stack")
                .AddRow(callStackTbl, varStackTbl));
        cpuTable.AddRow(pgmTbl, dbgTbl);

        AnsiConsole.Clear();
        AnsiConsole.Write(cpuTable);
    }
}