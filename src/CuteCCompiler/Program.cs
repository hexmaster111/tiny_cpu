using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CuteCCompiler;

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int c;
            int a = 42;
            int b = 69;

            fn main():void{
                int x = 2;
                int y = a + b;
            }
            """;


        var words = CuteCWordSplitter.Wordify(input);
        var tokens = CuteCTokenizer.Tokenize(words);
        var rootToken = CuteCLexer.Lex(tokens);

        CuteCVisualisation.DrawCompileSteps(words, tokens);
        Debugger.Break();
    }
}

public class CuteLexToken
{
    public List<CuteLexToken> Body { get; set; }
}

public static class CuteCLexer
{
    public static CuteLexToken Lex(List<CuteToke> tokes)
    {
        var ts = new TokenStream(tokes);
        var ns = new Stack<string>();
        ns.Push("global"); //Everything starts in the global ns

        while (ts.Next(out var token))
        {
            
        }

        var verify = ns.Pop();
        if (verify != "global") throw new Exception("Program did not end in global ns");
        
        throw new NotImplementedException();
    }
}

public class TokenStream
{
    private readonly List<CuteToke> _tokens;
    public TokenStream(List<CuteToke> tokens) => _tokens = tokens;
    public int Pos { get; private set; }
    public bool EndOfStream => Pos >= _tokens.Count;

    public CuteToke? Peek()
    {
        return _tokens[Pos + 1];
    }

    public bool Next([NotNullWhen(true)] out CuteToke? cuteToke)
    {
        cuteToke = null;
        if (EndOfStream) return false;
        cuteToke = _tokens[Pos++];
        return true;
    }
}