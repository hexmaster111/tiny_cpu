using System.Numerics;
using ImGuiNET;

namespace TinyNodes;

//sample from https://gist.github.com/ocornut/7e9b3ec566a333d725d4
public class NodeRenderer
{
    public NodeEditorStyle CurrentNodeEditorStyle = new();

    // State
    private List<Node> _nodes = new();
    private List<NodeLink> _links = new();
    private Vector2 _scrolling = new Vector2(0.0f, 0.0f);
    private bool _inited = false;
    private bool _showGrid = false;
    private int _nodeSelected { get; set; } = -1;

    public void ShowExampleNodeGraph(ref bool opened)
    {
        ImGui.SetNextWindowSize(new Vector2(700, 600), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Example: Custom Node Graph", ref opened))
        {
            ImGui.End();
            return;
        }


        // Initialization
        var io = ImGui.GetIO();
        if (!_inited)
        {
            _nodes.Add(new Node(0, "MainTex", new Vector2(40, 50), 0.5f, new Vector4(255, 100, 100, 255), 1, 1));
            _nodes.Add(new Node(1, "BumpMap", new Vector2(40, 150), 0.42f, new Vector4(200, 100, 200, 255), 1, 1));
            _nodes.Add(new Node(2, "Combine", new Vector2(270, 80), 1.0f, new Vector4(0, 200, 100, 255), 2, 2));
            _links.Add(new NodeLink(0, 0, 2, 0));
            _links.Add(new NodeLink(1, 0, 2, 1));
            _inited = true;
        }

        // Draw a list of nodes on the left side
        bool openContextMenu = false;

        int nodeHoveredInList = -1;
        int nodeHoveredInScene = -1;

        ImGui.BeginChild("node_list", new Vector2(100, 0));
        ImGui.Text("Nodes");
        ImGui.Separator();
        for (var nodeIdx = 0; nodeIdx < _nodes.Count; nodeIdx++)
        {
            var node = _nodes[nodeIdx];
            ImGui.PushID(node.ID);
            if (ImGui.Selectable(node.Name, node.ID == _nodeSelected))
                _nodeSelected = node.ID;
            if (ImGui.IsItemHovered())
            {
                nodeHoveredInList = node.ID;
                openContextMenu |= ImGui.IsMouseClicked(ImGuiMouseButton.Right);
            }

            ImGui.PopID();
        }

        ImGui.EndChild();

        ImGui.SameLine();
        ImGui.BeginGroup();

        const float nodeSlotRadius = 4.0f;
        var nodeWindowPadding = new Vector2(8.0f, 8.0f);

        // Create our child canvas
        ImGui.Text($"Hold middle mouse button to scroll ({_scrolling.X}, {_scrolling.Y})");
        // ImGui.SameLine(ImGui.GetWindowWidth() - 100);
        // ImGui.Checkbox("Show grid", ref _showGrid);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(1, 1));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CurrentNodeEditorStyle.BackgroundColor);
        ImGui.BeginChild("scrolling_region", new Vector2(0, 0), true,
            ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
        ImGui.PopStyleVar(); // WindowPadding
        ImGui.PushItemWidth(120.0f);

        var offset = ImGui.GetCursorScreenPos() + _scrolling;
        var drawList = ImGui.GetWindowDrawList();

        // Display grid
        if (_showGrid)
        {
            var gridColor = CurrentNodeEditorStyle.GridColor;
            const float gridSz = 64.0f;
            var winPos = ImGui.GetCursorScreenPos();
            var canvasSz = ImGui.GetWindowSize();
            for (var x = (_scrolling.X % gridSz); x < canvasSz.X; x += gridSz)
                drawList.AddLine(new Vector2(x, 0.0f) + winPos, new Vector2(x, canvasSz.Y) + winPos,
                    ImGui.GetColorU32(gridColor));
            for (var y = _scrolling.Y % gridSz; y < canvasSz.Y; y += gridSz)
                drawList.AddLine(new Vector2(0.0f, y) + winPos, new Vector2(canvasSz.X, y) + winPos,
                    ImGui.GetColorU32(gridColor));
        }


        // Display links
        drawList.ChannelsSplit(2);
        drawList.ChannelsSetCurrent(0); // Background
        for (var linkIdx = 0; linkIdx < _links.Count; linkIdx++)
        {
            var link = _links[linkIdx];
            var nodeInp = _nodes[link.InputIdx];
            var nodeOut = _nodes[link.OutputIdx];
            var p1 = offset + nodeInp.GetOutputSlotPos(link.InputSlot);
            var p2 = offset + nodeOut.GetInputSlotPos(link.OutputSlot);
            drawList.AddBezierCubic(p1, p1 + new Vector2(+50, 0), p2 + new Vector2(-50, 0), p2,
                CurrentNodeEditorStyle.WireColor, 3.0f);
        }

        // Display nodes
        for (var nodeIdx = 0; nodeIdx < _nodes.Count; nodeIdx++)
        {
            var node = _nodes[nodeIdx];
            ImGui.PushID(node.ID);
            var nodeRectMin = offset + node.Pos;

            // Display node contents first
            drawList.ChannelsSetCurrent(1); // Foreground
            var oldAnyActive = ImGui.IsAnyItemActive();
            ImGui.SetCursorScreenPos(nodeRectMin + nodeWindowPadding);
            ImGui.BeginGroup(); // Lock horizontal position
            ImGui.Text(node.Name);
            ImGui.SliderFloat("##value", ref node.Value, 0.0f, 1.0f, $"Alpha {node.Value}");
            ImGui.ColorEdit4("##color", ref node.Color);
            ImGui.EndGroup();

            // Save the size of what we have emitted and whether any of the widgets are being used
            var nodeWidgetsActive = (!oldAnyActive && ImGui.IsAnyItemActive());
            node.Size = ImGui.GetItemRectSize() + nodeWindowPadding + nodeWindowPadding;
            var nodeRectMax = nodeRectMin + node.Size;

            // Display node box
            drawList.ChannelsSetCurrent(0); // Background
            ImGui.SetCursorScreenPos(nodeRectMin);
            ImGui.InvisibleButton("node", node.Size);
            if (ImGui.IsItemHovered())
            {
                nodeHoveredInScene = node.ID;
                openContextMenu |= ImGui.IsMouseClicked(ImGuiMouseButton.Right);
            }

            var nodeMovingActive = ImGui.IsItemActive();
            if (nodeWidgetsActive || nodeMovingActive)
                _nodeSelected = node.ID;
            if (nodeMovingActive && ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                node.Pos = node.Pos + io.MouseDelta;

            var nodeBgColor =
                (nodeHoveredInList == node.ID || nodeHoveredInScene == node.ID ||
                 (nodeHoveredInList == -1 && _nodeSelected == node.ID))
                    ? CurrentNodeEditorStyle.NodeHoveredBackgroundColor
                    : CurrentNodeEditorStyle.NodeBackgroundColor;
            drawList.AddRectFilled(nodeRectMin, nodeRectMax, nodeBgColor, 4.0f);
            drawList.AddRect(nodeRectMin, nodeRectMax, CurrentNodeEditorStyle.NodeBorderColor
                , 4.0f);
            for (var slotIdx = 0; slotIdx < node.InputsCount; slotIdx++)
                drawList.AddCircleFilled(offset + node.GetInputSlotPos(slotIdx), nodeSlotRadius,
                    CurrentNodeEditorStyle.NodePinIn);
            for (var slotIdx = 0; slotIdx < node.OutputsCount; slotIdx++)
                drawList.AddCircleFilled(offset + node.GetOutputSlotPos(slotIdx), nodeSlotRadius,
                    CurrentNodeEditorStyle.NodePinOut);

            ImGui.PopID();
        }

        drawList.ChannelsMerge();

        // Open context menu
        if (ImGui.IsMouseReleased(ImGuiMouseButton.Right))
        {
            openContextMenu = true;
            //if the mouse is not over a node, clear the selection
        }

        if (ImGui.IsKeyDown(ImGuiKey.Escape))
        {
            _nodeSelected = nodeHoveredInList = nodeHoveredInScene = -1;
        }

        if (openContextMenu)
        {
            ImGui.OpenPopup("context_menu");
            if (nodeHoveredInList != -1)
                _nodeSelected = nodeHoveredInList;
            if (nodeHoveredInScene != -1)
                _nodeSelected = nodeHoveredInScene;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(8, 8));
        if (ImGui.BeginPopup("context_menu"))
        {
            Node? node = _nodeSelected != -1 ? _nodes[_nodeSelected] : null;
            var scenePos = ImGui.GetMousePosOnOpeningCurrentPopup() - offset;
            if (node != null)
            {
                ImGui.Text($"Node '{node.Name}'");
                ImGui.Separator();
                if (ImGui.MenuItem("Rename..", null, false, false)) ;
                if (ImGui.MenuItem("Delete", null, false, false)) ;
                if (ImGui.MenuItem("Copy", null, false, false)) ;
            }
            else
            {
                if (ImGui.MenuItem("Add"))
                    _nodes.Add(
                        new Node(_nodes.Count, "New node", scenePos, 0.5f, new Vector4(100, 100, 200, 255), 2, 2));
                if (ImGui.MenuItem("Paste", null, false, false)) ;
            }

            ImGui.EndPopup();
        }

        ImGui.PopStyleVar();

        // Scrolling
        if (ImGui.IsWindowHovered() && !ImGui.IsAnyItemActive() && ImGui.IsMouseDragging(ImGuiMouseButton.Middle, 0.0f))
            _scrolling = _scrolling + io.MouseDelta;

        ImGui.PopItemWidth();
        ImGui.EndChild();
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
        ImGui.EndGroup();

        ImGui.End();
    }

    // Dummy data structure provided for the example.
    // Note that we storing links as indices (not ID) to make example code shorter.
    private class Node
    {
        public int ID;
        public string Name;
        public Vector2 Pos, Size;
        public float Value;
        public Vector4 Color;
        public int InputsCount, OutputsCount;

        public Node(int id, string name, Vector2 pos, float value, Vector4 color, int inputsCount, int outputsCount)
        {
            ID = id;
            Name = name;
            Pos = pos;
            Value = value;
            Color = color;
            InputsCount = inputsCount;
            OutputsCount = outputsCount;
        }

        public Vector2 GetInputSlotPos(int slotNo)
        {
            return new Vector2(Pos.X, Pos.Y + Size.Y * ((float)slotNo + 1) / ((float)InputsCount + 1));
        }

        public Vector2 GetOutputSlotPos(int slotNo)
        {
            return new Vector2(Pos.X + Size.X, Pos.Y + Size.Y * ((float)slotNo + 1) / ((float)OutputsCount + 1));
        }
    }

    private class NodeLink
    {
        public NodeLink(int inputIdx, int inputSlot, int outputIdx, int outputSlot)
        {
            InputIdx = inputIdx;
            InputSlot = inputSlot;
            OutputIdx = outputIdx;
            OutputSlot = outputSlot;
        }

        public int InputIdx, InputSlot, OutputIdx, OutputSlot;
    }
}

public struct NodeEditorStyle
{
    public NodeEditorStyle()
    {
        NodeBorderColor = 0x80806e6e;
        NodeHoveredBackgroundColor = 0x33fa9642;
        NodeBackgroundColor = 0xf00f0f0f;
        NodePinIn = 0xfffa9642;
        NodePinOut = 0xfffa9642;
        BackgroundColor = 0xff2d2d2d;
        GridColor = 0x80806e6e;
        WireColor = 0xff00b3e6;
    }

    public void ShowEditWindow()
    {
        if (ImGui.Begin("Node Style"))
        {
            Vector4 vec_NodeBorderColor = ImGui.ColorConvertU32ToFloat4(NodeBorderColor);
            Vector4 vec_NodeHoveredBackgroundColor = ImGui.ColorConvertU32ToFloat4(NodeHoveredBackgroundColor);
            Vector4 vec_NodeBackgroundColor = ImGui.ColorConvertU32ToFloat4(NodeBackgroundColor);
            Vector4 vec_NodePinIn = ImGui.ColorConvertU32ToFloat4(NodePinIn);
            Vector4 vec_NodePinOut = ImGui.ColorConvertU32ToFloat4(NodePinOut);
            Vector4 vec_BackgroundColor = ImGui.ColorConvertU32ToFloat4(BackgroundColor);
            Vector4 vec_GridColor = ImGui.ColorConvertU32ToFloat4(GridColor);
            Vector4 vec_WireColor = ImGui.ColorConvertU32ToFloat4(WireColor);


            if (ImGui.Button("Copy to clipboard"))
            {
                ImGui.SetClipboardText($"{nameof(NodeBorderColor)}=0x{NodeBorderColor:x4}" +
                                       $"{nameof(NodeHoveredBackgroundColor)}=0x{NodeHoveredBackgroundColor:x4}" +
                                       $"{nameof(NodeBackgroundColor)}=0x{NodeBackgroundColor:x4}" +
                                       $"{nameof(NodePinIn)}=0x{NodePinIn:x4}" +
                                       $"{nameof(NodePinOut)}=0x{NodePinOut:x4}" +
                                       $"{nameof(BackgroundColor)}=0x{BackgroundColor:x4}" +
                                       $"{nameof(GridColor)}=0x{GridColor:x4}" +
                                       $"{nameof(WireColor)}=0x{WireColor:x4}");
            }


            ImGui.ColorEdit4("Node Border", ref vec_NodeBorderColor);
            ImGui.ColorEdit4("Node Hovered Background", ref vec_NodeHoveredBackgroundColor);
            ImGui.ColorEdit4("Node Background", ref vec_NodeBackgroundColor);
            ImGui.ColorEdit4("Node Pin In", ref vec_NodePinIn);
            ImGui.ColorEdit4("Node Pin Out", ref vec_NodePinOut);
            ImGui.ColorEdit4("Background", ref vec_BackgroundColor);
            ImGui.ColorEdit4("Grid", ref vec_GridColor);
            ImGui.ColorEdit4("Wire", ref vec_WireColor);
            ImGui.End();

            NodeBorderColor = ImGui.ColorConvertFloat4ToU32(vec_NodeBorderColor);
            NodeHoveredBackgroundColor = ImGui.ColorConvertFloat4ToU32(vec_NodeHoveredBackgroundColor);
            NodeBackgroundColor = ImGui.ColorConvertFloat4ToU32(vec_NodeBackgroundColor);
            NodePinIn = ImGui.ColorConvertFloat4ToU32(vec_NodePinIn);
            NodePinOut = ImGui.ColorConvertFloat4ToU32(vec_NodePinOut);
            BackgroundColor = ImGui.ColorConvertFloat4ToU32(vec_BackgroundColor);
            GridColor = ImGui.ColorConvertFloat4ToU32(vec_GridColor);
            WireColor = ImGui.ColorConvertFloat4ToU32(vec_WireColor);
        }
    }

    public uint NodeBorderColor;
    public uint NodeHoveredBackgroundColor;
    public uint NodeBackgroundColor;
    public uint NodePinIn;
    public uint NodePinOut;
    public uint BackgroundColor;
    public uint GridColor;
    public uint WireColor;
}