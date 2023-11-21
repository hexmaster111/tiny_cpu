using System.Diagnostics.CodeAnalysis;

namespace TinyAssemblerLib;

public readonly struct HecFile
{
    private readonly byte[] _fileBytes;
    private HecFile(byte[] fileBytes) => _fileBytes = fileBytes;
    public byte[] MagicNumber => _fileBytes[..8];
    public byte[] CodeEndOffsetBytes => _fileBytes[8..12];
    public int CODE_END_OFFSET => BitConverter.ToInt32(CodeEndOffsetBytes);
    public byte[] TinyCpuCode => _fileBytes[CODE_END_OFFSET..];


    private static byte[] GetHeader()
    {
        var offsetBytes = BitConverter.GetBytes(MAGIC_NUMBER.Length + sizeof(int));
        return MAGIC_NUMBER.Concat(offsetBytes).ToArray();
    }

    public static HecFile New(byte[] byteCode)
    {
        return Load(GetHeader().Concat(byteCode).ToArray());
    }

    public static bool TryLoad(
        string path,
        [NotNullWhen(true)] out HecFile? file,
        [NotNullWhen(false)] out HecFileLoadException? loadException
    )
    {
        try
        {
            file = Load(path);
            loadException = null;
            return true;
        }
        catch (HecFileLoadException ex)
        {
            file = null;
            loadException = ex;
            return false;
        }
    }


    public static HecFile Load(byte[] fileBytes)
    {
        var hecFile = new HecFile(fileBytes);
        Validate(hecFile); //Validate will throw LoadExcpetions 
        return hecFile;
    }

    public static HecFile Load(string path) => Load(File.ReadAllBytes(path));

    private static void Validate(HecFile f)
    {
        try
        {
            var a = f.MagicNumber;
            if (!a.SequenceEqual(MAGIC_NUMBER)) throw new HecFileLoadException();
        }
        catch
        {
            throw new HecFileLoadException("INVALID FILE TYPE");
        }
    }

    private static readonly byte[] MAGIC_NUMBER = { 0xFA, 0xDD, 0xED, 0xD0, 0x67, 0x00, 0x00, 0x00 };

    public class HecFileLoadException : Exception
    {
        public HecFileLoadException(string message) : base(message)
        {
        }

        public HecFileLoadException()
        {
        }
    }

    public byte[] GetFileBytes() => _fileBytes;
}