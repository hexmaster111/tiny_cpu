namespace CuteCCompiler;

public enum CuteTokenKind
{
    ERROR,
    Type,
    VarName,
    EndLine,
    Assignment,
    Function,
    OfType,
    TypedValue,
    OpenParen,
    CloseParen,
    OpenBracket,
    CloseBracket,
    Comma,
    __Math,
    Add,
    Sub,
    Div,
    Mut,
}

public static class CuteTokenKindExt
{
    public static CuteTokenKind[] MathTokens => new[]
        { CuteTokenKind.Add, CuteTokenKind.Sub, CuteTokenKind.Div, CuteTokenKind.Mut };

    public static CuteTokenKind[] ValueTypes => new[] { CuteTokenKind.VarName, CuteTokenKind.TypedValue };
}