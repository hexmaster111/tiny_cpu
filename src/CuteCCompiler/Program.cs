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
            int a;
            int b;

            a = 42;
            b = 69

            fn main():
            {
                int x;
                int y;
            
                x = 2
                y = a + b;
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
    public CuteLexToken(CuteLexTokenKind kind, CuteToke?[]? parts, string nameSpace)
    {
        Kind = kind;
        Parts = parts;
        NameSpace = nameSpace;
    }


    public string NameSpace { get; set; }

    public CuteLexTokenKind Kind { get; set; }
    public List<CuteLexToken> Body { get; set; } = new();
    public CuteToke?[]? Parts { get; set; }
}

public enum CuteLexTokenKind
{
    VarDef,
    VarAssignment
}

public static class CuteCLexer
{
    public static CuteLexToken Lex(List<CuteToke> tokes)
    {
        var ts = new TokenStream(tokes);
        var ns = new Stack<string>();
        var reg = new List<CuteLexToken>();
        ns.Push("global"); //Everything starts in the global ns

        while (ts.Next(
                   out var t0,
                   out var t1,
                   out var t2,
                   out var t3))
        {
            var isVarDec = (IsToken(CuteTokenKind.Type, t0) &&
                            IsToken(CuteTokenKind.VarName, t1) &&
                            IsToken(CuteTokenKind.EndLine, t2));

            //VarName Assignment [read in] endLine
            var isVarAssignment = (IsToken(CuteTokenKind.VarName, t0) &&
                                   IsToken(CuteTokenKind.Assignment, t1));

            var eat = 0;
            if (isVarDec) eat = 2;
            else if (isVarAssignment) eat = 1;
            for (int i = 0; i < eat; i++) ts.Next();

            if (isVarDec)
                reg.Add(new CuteLexToken(CuteLexTokenKind.VarDef, new[] { t0, t1, t2 }, GetCurrentVarNameSpace()));
            // else if (isVarAssignment)
            // reg.Add(new CuteLexToken(CuteLexTokenKind.VarDef, ReadVarAsnParts(), GetCurrentVarNameSpace()));
            continue;
            bool IsToken(CuteTokenKind t, CuteToke? ct) => ct?.Kind == t;
            string GetCurrentVarNameSpace() => ns!.Peek();
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

    public CuteToke? Peek(int dist)
    {
        if (Pos + dist >= 0 && Pos + dist < _tokens.Count) return _tokens[Pos + dist];
        return null;
    }

    public bool Next
        (out CuteToke? cuteToke, out CuteToke? peek0, out CuteToke? peek1, out CuteToke? peek2)
    {
        cuteToke = null;
        peek0 = null;
        peek1 = null;
        peek2 = null;

        if (EndOfStream) return false;
        cuteToke = Peek(0);
        peek0 = Peek(1);
        peek1 = Peek(2);
        peek2 = Peek(3);

        return Next();
    }

    public bool Next(out CuteToke? cuteToke)
    {
        cuteToke = null;
        if (EndOfStream) return false;
        cuteToke = Peek(0);
        return Next();
    }

    public bool Next()
    {
        Pos++;
        return !EndOfStream;
    }
}