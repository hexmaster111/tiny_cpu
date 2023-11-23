using Spectre.Console;

var input =
    """
    int c;
    int a = 42;
    int b = 69;

    void main(int num){
        int x = 2;
        int y = a + b;
    }
    """;

List<CuteToke> tokens = new();
List<string> documentTypeNames = new() { "int", "void" };
var next = input!;
while (!string.IsNullOrWhiteSpace(next))
{
    var (nxt, word) = ReadWord(next);
    next = nxt;
    Console.WriteLine(word);
}

return;


(string next, string word) ReadWord(string str)
{
    List<char> wordBuff = new();
    var sr = new StringReader(str);
    var dn = false;
    var first = true;
    while (!dn)
    {
        var peek = sr.Peek();
        if (peek is
            ';' or '(' or ')' or ',' or '+' or '-' or '\t' or
            '\r' or '\n' or '/' or '*' or '}' or '{' or ' ' or -1)
        {
            if (first && peek is not ('\t' or ' ' or -1)) wordBuff.Add((char)sr.Read());
            dn = true;
            continue;
        }

        wordBuff.Add((char)sr.Read());
        first = false;
    }

    return (sr.ReadToEnd(), new string(wordBuff.ToArray()));
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