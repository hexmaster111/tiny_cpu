namespace TinyNodes;

internal static class OpenDocument
{
    public static Document Document { get; set; } = new Document()
    {
        Name = "MyFirstDocument",
        NameSpaces =
        {
            new NameSpace()
            {
                Name = "MyFirstProgram",
                Functions =
                {
                    new Function() { Name = "Main" },
                    new Function() { Name = "Init" },
                    new Function() { Name = "Update" },
                    new Function() { Name = "Render" },
                    new Function() { Name = "Shutdown" }
                }
            },
            new NameSpace()
            {
                Name = "MyHelperLibrary",
                Functions =
                {
                    new Function() { Name = "MyHelperFunction" },
                    new Function() { Name = "MyHelperFunction2" },
                },
                NameSpaces =
                {
                    new NameSpace()
                    {
                        Name = "Base Libs",
                        Functions = { new Function() { Name = "read" } },
                    }
                }
            }
        }
    };
}