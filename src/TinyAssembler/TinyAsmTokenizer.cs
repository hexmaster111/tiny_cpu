using System.Collections.Immutable;
using TinyCpuLib;

namespace TinyAssembler;

public class TinyAsmTokenizer
{
    public string[] InputFileLine { get; }

    public TinyAsmTokenizer(string[] inputFileLines)
    {
        InputFileLine = inputFileLines;
    }

    public ImmutableArray<Token> Nom() => (
        from line in InputFileLine
        where !string.IsNullOrWhiteSpace(line)
        select Token.FromLine(line)
        into tok
        where tok != null
        select tok
    ).ToImmutableArray();

    public delegate void ErrorHandlerDelegate();

    public sealed record Token(
        Token.TokenType Type,
        Token.ArgumentType ArgumentZeroType = Token.ArgumentType.NONE,
        Token.ArgumentType ArgumentOneType = Token.ArgumentType.NONE,
        string ArgumentZeroData = "",
        string ArgumentOneData = ""
    )
    {
        public static Token? FromLine(string line)
        {
            var commentOut = line.Split(';');
            var cmdParts = commentOut[0].Split(' ', StringSplitOptions.TrimEntries |
                                                    StringSplitOptions.RemoveEmptyEntries);

            return FromParts(cmdParts);

            Token? FromParts(string[] parts)
            {
                if (!parts.Any()) return null;

                if (!Enum.TryParse(parts[0], true, out TokenType cmd))
                {
                    throw new TokenParseException(line, TokenParseException.Reason.UnableToParseTokenType,
                        $"\"{parts[0]}\" Invalid Parse ");
                }

                if (cmd == TokenType.NONE)
                    throw new TokenParseException(line, TokenParseException.Reason.UnableToParseTokenType,
                        "none is not a valid Token");
                var nextParts = parts[1..];
                return cmd switch
                {
                    TokenType.NOOP => ReadNoopToken(nextParts),
                    TokenType.HALT => ReadHaltToken(nextParts),
                    TokenType.RET => ReadRetToken(nextParts),
                    TokenType.NONE => throw new Exception("Token Parse got a NONE, this is invalid"),
                    _ => ReadToken(nextParts, cmd)
                };

                Token ReadRetToken(string[] strings) => new Token(TokenType.RET);

                Token ReadNoopToken(string[] strings) => new(TokenType.NOOP);

                Token ReadHaltToken(string[] strings) => new(TokenType.HALT);

                ArgumentType GetTokenType(string token,
                    out RegisterIndex? registerIndex,
                    out int? constantOut,
                    out string? strOut)
                {
                    registerIndex = null;
                    constantOut = null;
                    strOut = null;

                    if (char.IsNumber(token[0]))
                    {
                        constantOut = Convert.ToInt32(token, 16);
                        return ArgumentType.CONST;
                    }

                    if (Enum.TryParse<RegisterIndex>(token, out var ourRegIndex))
                    {
                        registerIndex = ourRegIndex;
                        return ArgumentType.REGISTER;
                    }

                    strOut = token;
                    return ArgumentType.STR;
                }

                Token ReadToken(string[] strings, TokenType type)
                {
                    var ArgumentZeroType = Token.ArgumentType.NONE;
                    var ArgumentOneType = Token.ArgumentType.NONE;
                    var ArgZeroData = "";
                    var ArgOneData = "";


                    var expectedParts = type.ExpectedArgumentCount();

                    for (int i = 0; i < expectedParts; i++)
                    {
                        var argT = GetTokenType(nextParts[i],
                            out var registerIndex,
                            out var constantOut,
                            out var strOut);

                        switch (i)
                        {
                            case 0:
                                SetVars(ref ArgumentZeroType, ref ArgZeroData, registerIndex, constantOut, strOut,
                                    argT);
                                break;
                            case 1:
                                SetVars(ref ArgumentOneType, ref ArgOneData, registerIndex, constantOut, strOut, argT);
                                break;
                            default:
                                throw new ArgumentException("Too many args!");


                                void SetVars(ref ArgumentType argumentTypeRef, ref string argDataRef,
                                    RegisterIndex? ri,
                                    int? co,
                                    string? s, ArgumentType argumentType)
                                {
                                    argumentTypeRef = argumentType;
                                    switch (argumentType)
                                    {
                                        case ArgumentType.CONST:
                                            argDataRef = co.Value.ToString("X");
                                            break;
                                        case ArgumentType.REGISTER:
                                            argDataRef = ri.Value.ToString();
                                            break;
                                        case ArgumentType.STR:
                                            argDataRef = s.ToUpper();
                                            break;
                                        case ArgumentType.NONE:
                                        default:
                                            throw new ArgumentOutOfRangeException(nameof(argumentType), argumentType,
                                                null);
                                    }
                                }
                        }
                    }

                    return new Token(type, ArgumentZeroType, ArgumentOneType, ArgZeroData, ArgOneData);
                }
            }
        }

        public class TokenParseException : Exception
        {
            public string Line;
            public Reason Why;

            public TokenParseException(string line, Reason reason, string message) : base(message)
            {
                Line = line;
                Why = reason;
            }

            public enum Reason
            {
                UnableToParseTokenType,
            }
        }

        public enum TokenType
        {
            NONE,
            NOOP,
            SETREG,
            ADD,
            SUB,
            DIV,
            MUL,
            LBL,
            CALL,
            HALT,
            RET,
            PUSH,
            POP,
            INC,
            DEC,
            CMP
        }

        // @formatter:keep_existing_enum_arrangement true
        public enum ArgumentType { CONST, REGISTER, STR, NONE }
        // @formatter:keep_existing_enum_arrangement restore
    };
}