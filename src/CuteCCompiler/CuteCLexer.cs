using System.Diagnostics;

namespace CuteCCompiler;

public static class CuteCLexer
{
    public static void Lex(ProgramRoot rootToken)
    {
        var ts = new TokenStream(rootToken.ChildData.ToArray());
        Lex(rootToken, ts, "global");
    }

    private static void Lex(ICuteLexNode current, TokenStream ts, string ns)
    {
        var proceedAllBodyTokens = false;

        while (!proceedAllBodyTokens)
        {
            if (ts.EndOfStream)
            {
                proceedAllBodyTokens = true;
                continue;
            }


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
            Lex(child, new TokenStream(child.ChildData.ToArray()), ns + "::" + child.ProvidedNameSpace);
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