namespace CuteCCompiler;

public static class CuteCTokenizer
{
    //TODO: Before expanding on types more, a type scanning step must be added before running tokenize
    public static readonly List<string> DocumentTypeNames = new() { "int", "void" };
    public static bool IsTypeName(string str) => DocumentTypeNames.Contains(str);

    public static List<CuteToke> Tokenize(List<TokenWord> words)
    {
        List<CuteToke> tokens = new();
        foreach (var word in words)
        {
            var isTypeName = IsTypeName(word.Str);
            var isAdd = CuteCConst.MathAdd == word.Str;
            var isSub = CuteCConst.MathSub == word.Str;
            var isMul = CuteCConst.MathMul == word.Str;
            var isDiv = CuteCConst.MathDiv == word.Str;
            var isComma = CuteCConst.Comma == word.Str;
            var isOpenParen = CuteCConst.OpenParen == word.Str;
            var isCloseParen = CuteCConst.CloseParen == word.Str;
            var isOpenBracket = CuteCConst.OpenBracket == word.Str;
            var isCloseBracket = CuteCConst.CloseBracket == word.Str;
            var isEndLine = CuteCConst.LineEnd == word.Str;
            var isAssignment = CuteCConst.Assignment == word.Str;
            var isFunction = CuteCConst.Function == word.Str;
            var isOfType = CuteCConst.OfType == word.Str;
            var isTypedIntVal = int.TryParse(word.Str, out var typedIntVal);
            var isVarName = tokens.Any() && tokens.Last().Kind is CuteTokenKind.Type or CuteTokenKind.Function;

            if (isTypeName) tokens.Add(new CuteToke(CuteTokenKind.Type, word));
            else if (isAdd) tokens.Add(new CuteToke(CuteTokenKind.Add, word));
            else if (isSub) tokens.Add(new CuteToke(CuteTokenKind.Sub, word));
            else if (isMul) tokens.Add(new CuteToke(CuteTokenKind.Mut, word));
            else if (isDiv) tokens.Add(new CuteToke(CuteTokenKind.Div, word));
            else if (isComma) tokens.Add(new CuteToke(CuteTokenKind.Comma, word));
            else if (isOpenParen) tokens.Add(new CuteToke(CuteTokenKind.OpenParen, word));
            else if (isCloseParen) tokens.Add(new CuteToke(CuteTokenKind.CloseParen, word));
            else if (isOpenBracket) tokens.Add(new CuteToke(CuteTokenKind.OpenBracket, word));
            else if (isCloseBracket) tokens.Add(new CuteToke(CuteTokenKind.CloseBracket, word));
            else if (isEndLine) tokens.Add(new CuteToke(CuteTokenKind.EndLine, word));
            else if (isOfType) tokens.Add(new CuteToke(CuteTokenKind.OfType, word));
            else if (isAssignment) tokens.Add(new CuteToke(CuteTokenKind.Assignment, word));
            else if (isFunction) tokens.Add(new CuteToke(CuteTokenKind.Function, word));
            else if (isTypedIntVal) tokens.Add(new CuteToke(CuteTokenKind.TypedValue, word));
            else if (isVarName) tokens.Add(new CuteToke(CuteTokenKind.VarName, word));
            else tokens.Add(new CuteToke(CuteTokenKind.VarName, word));
        }

        return tokens;
    }
}