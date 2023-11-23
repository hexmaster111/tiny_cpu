namespace CuteCCompiler;

public enum CuteTokenKind
{
    ERROR,
    Type, VarName, EndLine, Assignment, Function, OfType,
    TypedValue,
    OpenParen, CloseParen,
    OpenBracket, CloseBracket,
    Comma,
    Add, Sub, Div, Mut
}