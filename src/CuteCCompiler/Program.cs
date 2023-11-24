using System.Diagnostics;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int c = 0;
            int a = 42;
            int b = 69;

            fn demo ( int val ) : int {
                return(val);
            }
            
            fn main (  ) : void
            {
                int x = a + b;
                int y = a * x;
            }

            main();
            """;


        var words = CuteCWordSplitter.Wordify(input);
        var tokens = CuteCTokenizer.Tokenize(words);
        var rootToken = new ProgramRoot(tokens);
        CuteCLexer.Lex(rootToken);

        CuteCVisualisation.DrawCompileSteps(input, words, tokens, rootToken);
        Debugger.Break();
    }
}