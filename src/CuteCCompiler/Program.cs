using System.Diagnostics;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int c;
            int a;
            int b;

            a = 42;
            b = 69;

            fn add ( int a, int b ) : int
            {
                ret(a + b);
            }

            fn main(  ) : void
            {
                int x;
                int y;
            
                x = add ( a, b );
                y = a + b;
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