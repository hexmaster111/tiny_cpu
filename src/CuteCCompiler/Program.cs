namespace CuteCCompiler;

public record Word(string Data, int StartChar);

internal class Program
{
    public static void Main(string[] args)
    {
        var input =
            """
            int c;
            int a = 42;
            int b = 69;
            int someVar = 1+(69*2);

            void main(int num){
                int x = 2;
                int y = a + b;
            }
            """;

        var ws = new CuteCWordSplitter();
        ws.Parse(input);
        var words = ws.Words;

        List<string> documentTypeNames = new() { "int", "void" };
        List<CuteToke> tokens = new();
        return;
    }
}

record CuteToke(CuteTokenKind Kind, string Value);

enum CuteTokenKind
{
    NONE,
    Type, VarName, EndLine, Assignment, TypedIntValue,
    OpenParen, CloseParen,
    OpenBracket, CloseBracket,
    Comma,
    Add, Sub, Div, Mut
}