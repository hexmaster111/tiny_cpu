namespace CuteCCompiler;

public record CuteToke(CuteTokenKind Kind, TokenWord Data)
{
    public static readonly CuteToke Void = new CuteToke(CuteTokenKind.Type, new TokenWord("void", -1));
}