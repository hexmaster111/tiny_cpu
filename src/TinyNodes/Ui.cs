using System.Numerics;
using ImGuiNET;

namespace TinyNodes;

internal static class Ui
{
    internal static bool SwDemoWindow = false;
    internal static bool SwFunctionBrowser = true;


    //Ui overlay drawing
    /*
     * Images
     * Raylib textures can be drawn in ImGui using the following functions
     *
     * rlImGui.Image(Texture2D image);
     * rlImGui.ImageSize(Texture2D image, int width, int height);
     * rlImGui.ImageRect(Texture2D image, int destWidth, int destHeight, Rectangle sourceRect);
     * rlImGui.ImageRenderTexture(RenderTexture2D image);
     * Image Buttons
     * rlImGui.ImageButton(System.String name, Texture2D image);
     * rlImGui.ImageButtonSize(System.String name, Texture2D image, Vector2 size);
     *
     */

    public static void Render()
    {
        if (SwDemoWindow) ImGui.ShowDemoWindow(ref SwDemoWindow);
        ImGui.DockSpaceOverViewport();
        RenderMainMenuToolBar();
        if (SwFunctionBrowser) RenderFunctionBrowser(ref SwFunctionBrowser);
    }

    //Tools Window / Functions Window
    private static void RenderFunctionBrowser(ref bool showToolsWindow)
    {
        if (!ImGui.Begin("Tools", ref showToolsWindow, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoTitleBar))
            return;


        if (ImGui.TreeNode(OpenDocument.Document.Name + "##" + OpenDocument.Document.GetHashCode()))
        {
            foreach (var nameSpace in OpenDocument.Document.NameSpaces)
            {
                RecursiveDraw(nameSpace);
            }


            ImGui.TreePop();
            return;
        }


        return;

        void RecursiveDraw(NameSpace nameSpace)
        {
            if (!ImGui.TreeNode(nameSpace.Name + "##" + nameSpace.GetHashCode())) return;


            StyleLabel();
            ImGui.Text("Functions");
            PopLabel();

            foreach (var function in nameSpace.Functions)
            {
                if (ImGui.Selectable(function.Name + "##" + function.GetHashCode()))
                {
                    Console.WriteLine(function.Name);
                }
            }

            if(nameSpace.NameSpaces.Count == 0) return;
            StyleLabel();
            ImGui.Text("NameSpaces");
            PopLabel();

            foreach (var nameSpace1 in nameSpace.NameSpaces)
            {
                RecursiveDraw(nameSpace1);
            }

            ImGui.TreePop();
        }

        return;

        void StyleLabel()
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 0, 1));
        }

        void PopLabel()
        {
            ImGui.PopStyleColor();
        }
    }


    private static void RenderMainMenuToolBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Open", "Ctrl+O")) ;
                if (ImGui.MenuItem("Save", "Ctrl+S")) ;
                if (ImGui.MenuItem("Close", "Ctrl+W")) ;
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Edit"))
            {
                if (ImGui.MenuItem("Undo", "Ctrl+Z")) ;
                if (ImGui.MenuItem("Redo", "Ctrl+Y", false, false)) ;
                ImGui.Separator();
                if (ImGui.MenuItem("Cut", "Ctrl+X")) ;
                if (ImGui.MenuItem("Copy", "Ctrl+C")) ;
                if (ImGui.MenuItem("Paste", "Ctrl+V")) ;
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("View"))
            {
                if (ImGui.MenuItem("Function Browser", "", SwFunctionBrowser)) SwFunctionBrowser = !SwFunctionBrowser;
                if (ImGui.MenuItem("Demo Window", "", SwDemoWindow)) SwDemoWindow = !SwDemoWindow;
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}