using System.Collections.Immutable;

namespace TinyAssembler;

public class TinyAsmAssembler
{
    public byte[] Assembly { get; private set; }

    public TinyAsmAssembler(ImmutableArray<TinyAsmTokenizer.Token> tokens)
    {
        Tokens = tokens;
        Assembly = Array.Empty<byte>();
    }

    public ImmutableArray<TinyAsmTokenizer.Token> Tokens { get; private set; }

    public byte[] Assemble()
    {

        var inst = Tokens.Select(AsmToken.New);
        
        return Assembly;
    }
}

internal record AsmToken(TinyAsmTokenizer.Token Token, byte[] Data)
{
    public static AsmToken New(TinyAsmTokenizer.Token token)
    {
        
    }
}