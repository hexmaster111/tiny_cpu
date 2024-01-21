using System.Collections.Immutable;
using TinyCpuLib;
using TinyExt;

namespace TinyAssemblerLib;

public class TinyAsmTokenizer
{
    public string[] InputFileLine { get; }

    public TinyAsmTokenizer(string[] inputFileLines)
    {
        InputFileLine = inputFileLines;
    }

    public ImmutableArray<Token> Nom() => InputFileLine
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .Select(line => line.ToUpper())
        .Select(Token.FromLine)
        .Where(token => token != null)
        .Select(token => token!)
        .ToImmutableArray();

    public delegate void ErrorHandlerDelegate();

    public sealed record Token(
        Token.TokenType Type,
        Token.ArgumentType ArgumentZeroType = Token.ArgumentType.NONE,
        Token.ArgumentType ArgumentOneType = Token.ArgumentType.NONE,
        string ArgumentZeroData = "",
        string ArgumentOneData = ""
    )
    {
        public override string ToString()
        {
            return Type + " " + ArgumentZeroData + " " + ArgumentOneData;
        }

        public static Token? FromLine(string line)
        {
            line = line.Trim();
            if (line.StartsWith(';')) return null;
            var commentSplit = line.Split(';');
            line = commentSplit[0];
            List<string> cmdParts = new();
            SplitLine(line, cmdParts);
            for (var i = 0; i < cmdParts.Count; i++) cmdParts[i] = cmdParts[i].Trim();
            cmdParts = cmdParts.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return FromParts(cmdParts.ToArray(), line);
        }

        private static void SplitLine(string line, List<string> cmdParts)
        {
            var inQuotes = false;
            string currentPart = "";
            foreach (var c in line)
            {
                if (c == '"')
                {
                    if (inQuotes)
                    {
                        currentPart += c;
                        cmdParts.Add(currentPart);
                        currentPart = "";
                        continue;
                    }

                    inQuotes = !inQuotes;
                }
                else if (c == ' ' && !inQuotes)
                {
                    cmdParts.Add(currentPart);
                    currentPart = "";
                }

                currentPart += c;
            }

            cmdParts.Add(currentPart);
        }

        private static Token? FromParts(string[] parts, string line)
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
                _ => ReadToken(nextParts, cmd, nextParts)
            };

            Token ReadRetToken(string[] strings) => new Token(TokenType.RET);

            Token ReadNoopToken(string[] strings) => new(TokenType.NOOP);

            Token ReadHaltToken(string[] strings) => new(TokenType.HALT);
        }

        private static Token ReadToken(string[] strings, TokenType type, string[]? nextParts)
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
                    out var fnNameOut,
                    out var strRegisterIndex,
                    out var strLiteralOut);

                switch (i)
                {
                    case 0:
                        SetVars(
                            ref ArgumentZeroType,
                            ref ArgZeroData,
                            registerIndex,
                            constantOut,
                            fnNameOut,
                            strLiteralOut,
                            strRegisterIndex,
                            argT);
                        break;
                    case 1:
                        SetVars(
                            ref ArgumentOneType,
                            ref ArgOneData,
                            registerIndex,
                            constantOut,
                            fnNameOut,
                            strLiteralOut,
                            strRegisterIndex,
                            argT);
                        break;
                    default:
                        throw new ArgumentException("Too many args!");
                }
            }

            return new Token(type, ArgumentZeroType, ArgumentOneType, ArgZeroData, ArgOneData);
        }

        private static void SetVars(
            ref ArgumentType argumentTypeRef,
            ref string argDataRef,
            IntRegisterIndex? ri,
            int? co,
            string? fnName,
            string? strLit,
            StrRegisterIndex? strRegisterIndex,
            ArgumentType argumentType
        )
        {
            argumentTypeRef = argumentType;
            switch (argumentType)
            {
                case ArgumentType.ConstInt:
                    argDataRef = co.Value.ToString("X");
                    break;
                case ArgumentType.IntRegister:
                    argDataRef = ri.Value.ToString();
                    break;
                case ArgumentType.FuncName:
                    argDataRef = fnName.ToUpper();
                    break;
                case ArgumentType.StrRegister:
                    argDataRef = strRegisterIndex.Value.ToString();
                    break;
                case ArgumentType.StrLiteral:
                    argDataRef = strLit;
                    break;

                case ArgumentType.NONE:
                default:
                    throw new ArgumentOutOfRangeException(nameof(argumentType), argumentType,
                        null);
            }
        }

        private static ArgumentType GetTokenType(
            string token,
            out IntRegisterIndex? registerIndex,
            out int? constantOut,
            out string? fnNameOut,
            out StrRegisterIndex? strRegisterIndex,
            out string? strLiteralOut
        )
        {
            registerIndex = null;
            constantOut = null;
            fnNameOut = null;
            strRegisterIndex = null;
            strLiteralOut = null;

            if (char.IsNumber(token[0]))
            {
                constantOut = Convert.ToInt32(token, 16);
                return ArgumentType.ConstInt;
            }

            if (Enum.TryParse<IntRegisterIndex>(token, out var ourRegIndex))
            {
                registerIndex = ourRegIndex;
                return ArgumentType.IntRegister;
            }

            if (Enum.TryParse<StrRegisterIndex>(token, out var strRegIndex))
            {
                strRegisterIndex = strRegIndex;
                return ArgumentType.StrRegister;
            }

            if (token.StartsWith("\"") && token.EndsWith("\""))
            {
                strLiteralOut = token[1..^1];
                return ArgumentType.StrLiteral;
            }

            fnNameOut = token;
            return ArgumentType.FuncName;
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
            CMP,
            JMP_EQ,
            JMP_NEQ,
            JMP_GTR,
            JMP_GEQ,
            JMP_LES,
            JMP_LEQ,
            JMP,
            MEM_READ,
            MEM_WRITE,
            SCCAT,
        }

        public enum ArgumentType
        {
            ConstInt,
            IntRegister,
            StrRegister,
            FuncName,
            StrLiteral,
            NONE
        }
    };
}