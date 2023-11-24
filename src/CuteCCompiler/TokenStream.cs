using System.Diagnostics;

namespace CuteCCompiler;

[DebuggerDisplay("{GetDebuggerString()}")]
public class TokenStream
{
    private readonly CuteToke[] _tokens;
    public TokenStream(CuteToke[] tokens) => _tokens = tokens;
    public int Pos { get; private set; }
    public bool EndOfStream => Pos >= _tokens.Length;

    public CuteToke? Peek(int dist)
    {
        if (Pos + dist >= 0 && Pos + dist < _tokens.Length) return _tokens[Pos + dist];
        return null;
    }


    public bool Next(out CuteToke? cuteToke)
    {
        cuteToke = null;
        if (EndOfStream) return false;
        cuteToke = Peek(0);
        return Next();
    }


    private string GetDebuggerString()
    {
        var s = $"Pos:{Pos} End:{EndOfStream} [";
        for (var i = 0; i < 3; i++)
        {
            var peek = Peek(i);
            if (peek == null) s += "(null)";
            else s += peek.Kind.ToString();
            s += ",";
        }

        s += "]";

        return s;
    }

    public bool Next()
    {
        bool last = !EndOfStream;
        Pos++;
        return last;
    }

    public CuteToke TakeOne()
    {
        if (EndOfStream) throw new InvalidOperationException("At end of stream!");
        Next(out CuteToke? t);
        Debug.Assert(t != null);
        return t;
    }
}