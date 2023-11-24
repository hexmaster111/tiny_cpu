using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.NetworkInformation;

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

            fn main(int a, void b):void
            {
                int x;
                int y;
            
                x = 2;
                y = a + b;
            }

            """;


        var words = CuteCWordSplitter.Wordify(input);
        var tokens = CuteCTokenizer.Tokenize(words);
        var rootToken = new ProgramRoot(tokens);
        CuteCLexer.Lex(rootToken);

        CuteCVisualisation.DrawCompileSteps(words, tokens);
        Debugger.Break();
    }
}

public interface ICuteLexNode
{
    /// Data to be used by next call to LEX
    public List<CuteToke> ChildData { get; }

    public List<CuteToke> Expression { get; }

    public CuteLexNodeKind Kind { get; }

    public List<ICuteLexNode> Children { get; set; }
    public string NameSpace { get; }
}

public class VarDef : ICuteLexNode
{
    public VarDef(TokenStream ts, string nameSpace)
    {
        var expr = ts.ReadEndLineType().ToArray();
        VariableType = expr[0];
        VariableName = expr[1];
        Expression = expr[2..].ToList();
        NameSpace = nameSpace;
    }

    public CuteToke VariableType { get; }
    public CuteToke VariableName { get; }
    public string NameSpace { get; }
    public List<CuteToke> Expression { get; }
    public List<CuteToke> ChildData { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarDefinition;
    public List<ICuteLexNode> Children { get; set; } = new();
}

public class FuncDef : ICuteLexNode
{
    public FuncDef(TokenStream ts, string ns)
    {
        NameSpace = ns;
        var fnKeyWord = ts.TakeOne();
        FuncName = ts.TakeOne();
        Args = ts.ReadParenBody();
        var ofTypeKeyWord = ts.TakeOne();
        Return = ts.TakeOne();
        ChildData = ts.ReadBracketBody();
    }

    public CuteToke FuncName { get; }
    public List<CuteToke> Args { get; }
    public CuteToke Return { get; }

    public List<CuteToke> ChildData { get; }
    public List<CuteToke> Expression { get; }
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.FuncDefinition;
    public List<ICuteLexNode> Children { get; set; } = new();
    public string NameSpace { get; }
}

public class VarAsi : ICuteLexNode
{
    public VarAsi(TokenStream ts, string ns)
    {
        NameSpace = ns;
        var exprTokens = ts.ReadEndLineType();
        var exprStream = new TokenStream(exprTokens.ToArray());
        VarBeingAssignedTo = exprStream.TakeOne();
        var keyWord = exprStream.TakeOne();
        if (keyWord.Kind != CuteTokenKind.Assignment) throw new Exception("Incorrect keyword!");
        Expression = exprStream.ReadEndLineType();
    }

    public CuteToke VarBeingAssignedTo { get; }
    public List<CuteToke> ChildData { get; } = new();
    public List<CuteToke> Expression { get; }
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.VarAssignment;
    public string NameSpace { get; }
    public List<ICuteLexNode> Children { get; set; } = new();
}

public class FuncCall : ICuteLexNode
{
    public FuncCall(TokenStream ts, string ns)
    {
        throw new NotImplementedException();
    }

    public List<CuteToke> ChildData { get; }
    public List<CuteToke> Expression { get; }
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.FuncCall;
    public List<ICuteLexNode> Children { get; set; } = new();
    public string NameSpace { get; }
}

public class ProgramRoot : ICuteLexNode
{
    public ProgramRoot(List<CuteToke> tokens) => ChildData = tokens;

    public List<CuteToke> ChildData { get; }
    public List<CuteToke> Expression { get; } = new();
    public CuteLexNodeKind Kind { get; } = CuteLexNodeKind.ProgramRoot;
    public List<ICuteLexNode> Children { get; set; } = new();
    public string NameSpace { get; } = ".";
}

public static class CuteCLexer
{
    public static void Lex(ProgramRoot rootToken)
    {
        var ts = new TokenStream(rootToken.ChildData.ToArray());
        Lex(rootToken, ts, "global");
    }

    public static void Lex(ICuteLexNode current, TokenStream ts, string ns)
    {
        var proceedAllBodyTokens = false;

        while (!proceedAllBodyTokens)
        {
            
            if (ts.EndOfStream)
            {
                proceedAllBodyTokens = true;
                continue;
            }
            
            Console.WriteLine(current.Kind);

            if (IsTokenPattern(CuteTokenKind.Type, CuteTokenKind.VarName))
            {
                var node = new VarDef(ts, ns);
                current.Children.Add(node);
            }
            else if (IsTokenPattern(CuteTokenKind.Function, CuteTokenKind.VarName, CuteTokenKind.OpenParen))
            {
                var node = new FuncDef(ts, ns);
                current.Children.Add(node);
            }
            else if (IsTokenPattern(CuteTokenKind.VarName, CuteTokenKind.Assignment))
            {
                var node = new VarAsi(ts, ns);
                current.Children.Add(node);
            }
            else if (IsTokenPattern(CuteTokenKind.VarName, CuteTokenKind.OpenParen))
            {
                var node = new FuncCall(ts, ns);
                current.Children.Add(node);
            }
            else throw new Exception("Unknown Pattern");

           

            continue;

            bool IsTokenPattern(params CuteTokenKind[] kinds) =>
                !kinds.Where((t, i) => ts.Peek(i)?.Kind != t).Any();
        }


        foreach (var child in current.Children)
        {
            Lex(child, new TokenStream(child.ChildData.ToArray()), ns + "::" + child.Kind); //TODO: real namespace
        }
    }


    // ends with a end line ;
    public static List<CuteToke> ReadEndLineType(this TokenStream ts)
    {
        var ret = new List<CuteToke>();
        while (ts.Next(out var t))
        {
            if (t == null) throw new Exception("missing endLine");
            ret.Add(t);
            if (t.Kind == CuteTokenKind.EndLine) break;
        }

        return ret;
    }


    // ( ... )
    public static List<CuteToke> ReadParenBody(this TokenStream ts) =>
        ReadBody(ts, CuteTokenKind.OpenParen, CuteTokenKind.CloseParen);

    // { ... }
    public static List<CuteToke> ReadBracketBody(this TokenStream ts) =>
        ReadBody(ts, CuteTokenKind.OpenBracket, CuteTokenKind.CloseBracket);

    static List<CuteToke> ReadBody(TokenStream ts, CuteTokenKind openKind, CuteTokenKind closeKind)
    {
        var ret = new List<CuteToke>();

        var g0 = ts.Next(out var op);
        if (op == null) throw new Exception("Unexpected EOF");
        if (op.Kind != openKind)
            throw new Exception($"Expected {openKind}, got {op.Kind} from {op.Data.Str}");


        var open = 1;
        var close = 0;


        while (open != close)
        {
            if (!ts.Next(out var token)) throw new Exception("Unexpected EOF");
            Debug.Assert(token != null);
            if (token.Kind == openKind)
            {
                open++;
                continue;
            }

            if (token.Kind == closeKind)
            {
                close++;
                continue;
            }

            ret.Add(token);
        }

        return ret;
    }
}