namespace CuteCCompiler;

public static class CuteCConst
{
    public static readonly char[] whiteSpace = { '\r', '\n', ' ', '\t' };
    public static readonly char[] specialChars = { '=', ';', '(', ')', '{', '}', '+', '-', '/', '*' };
    public static bool IsSpecial(char c) => specialChars.Contains(c);
    public static bool IsWhiteSpace(char c) => whiteSpace.Contains(c);
}

internal class CuteCWordSplitter
{

    private static bool IsCharNormal(char ch)
    {
        var a = CuteCConst.whiteSpace.Contains(ch);
        var b = CuteCConst.specialChars.Contains(ch);
        return !a && !b;
    }


    private string wordBuff = "";
    private bool IsWordBuffEmpty() => !wordBuff.Any();

    public List<Word> Words { get; private set; } = new();

    public void Parse(string input)
    {
        int c = 0;
        var sr = new StringReader(input);
        wordBuff = "";
        Words = new();
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
                    Words.Add(new Word(wordBuff, c));
                    Console.WriteLine(wordBuff);
                    wordBuff = "";
                }
            }

            if (peek == -1) break;
        }
    }
}