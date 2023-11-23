namespace CuteCCompiler;

public static class CuteCConst
{
    public static readonly char[] whiteSpace = { '\r', '\n', ' ', '\t' };

    public const string Assignment = "=";
    public const string LineEnd = ";";
    public const string OpenParen = "(";
    public const string CloseParen = ")";
    public const string OpenBracket = "{";
    public const string CloseBracket = "}";
    public const string MathAdd = "+";
    public const string MathSub = "-";
    public const string MathDiv = "/";
    public const string MathMul = "*";
    public const string Comma = ",";
    public const string Function = "fn";
    public const string OfType = ":";

    public static readonly string[] special =
    {
        Assignment, LineEnd, OpenParen, CloseParen, OpenBracket,
        CloseBracket, MathAdd, MathSub, MathDiv, MathMul, Comma,
        OfType
    };

    public static bool IsSpecial(char c) => special.Contains(c.ToString());
    public static bool IsWhiteSpace(char c) => whiteSpace.Contains(c);
}

public record TokenWord(string Str, int StartChar);

internal static class CuteCWordSplitter
{
    private static bool IsCharNormal(char ch)
    {
        var a = CuteCConst.whiteSpace.Contains(ch);
        var b = CuteCConst.special.Contains(ch.ToString());
        return !a && !b;
    }


    public static List<TokenWord> Wordify(string input)
    {
        string wordBuff = "";
        bool IsWordBuffEmpty() => !wordBuff.Any();
        int c = 0;
        var sr = new StringReader(input);
        wordBuff = "";
        var words = new List<TokenWord>();
        while (true)
        {
            var read = sr.Read();
            c++;
            var peek = sr.Peek();

            var readCh = (char)read;
            var peekCh = (char)peek;

            if (!CuteCConst.IsWhiteSpace(readCh))
            {
                wordBuff += readCh;
            }

            if (CuteCConst.IsWhiteSpace(readCh) || !IsCharNormal(peekCh) || peek == -1 || CuteCConst.IsSpecial(readCh))
            {
                if (!IsWordBuffEmpty())
                {
                    words.Add(new TokenWord(wordBuff, c));
                    wordBuff = "";
                }
            }

            if (peek == -1) break;
        }

        return words;
    }
}