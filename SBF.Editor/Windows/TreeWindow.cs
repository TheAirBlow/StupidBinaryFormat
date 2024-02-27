using System.Collections;
using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using SBF.Core;

namespace SBF.Editor.Windows;

public class TreeWindow : GuiWindow {
    /// <summary>
    /// Root element
    /// </summary>
    public TreeNode? RootNode;

    /// <summary>
    /// File Path
    /// </summary>
    public string Path = "";

    /// <summary>
    /// Creates an empty file (safe public versio)
    /// </summary>
    public void SafeCreateEmpty(ImGuiRenderer renderer) {
        if (RootNode == null) {
            CreateEmpty(); return;
        }

        var choice = new ChoiceWindow("Create empty",
            "A file is open already. Do you want to discard it?");
        choice.OnClosed += (_, yes) => {
            if (yes) CreateEmpty();
        };
        renderer.OpenWindow(choice);
    }
    
    /// <summary>
    /// Creates an empty file
    /// </summary>
    private void CreateEmpty() 
        => RootNode = new TreeNode("Root Element", new Dictionary<string, object>());

    /// <summary>
    /// Open a SBF file
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    /// <param name="path">Path to file</param>
    public void OpenFileSafe(ImGuiRenderer renderer, string path) {
        if (RootNode == null) {
            OpenFile(renderer, path); return;
        }
        
        var choice = new ChoiceWindow("Open file",
            "A file is open already. Do you want to discard it?");
        choice.OnClosed += (_, yes) => {
            if (yes) OpenFile(renderer, path);
        };
        renderer.OpenWindow(choice);
    }
    
    /// <summary>
    /// Open a SBF file
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    /// <param name="path">Path to file</param>
    private void OpenFile(ImGuiRenderer renderer, string path) {
        try {
            using var file = new FileStream(path, FileMode.Open, FileAccess.Read);
            RootNode = new TreeNode("Root Element", BinarySerializer.DeserializeRaw(file));
            Path = path;
        } catch (Exception e) {
            renderer.OpenWindow(new PopupWindow("Failed to build tree", e.ToString()));
        }
    }
    
    /// <summary>
    /// Draw the GUI
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        ImGui.SetNextWindowSize(ImGui.GetIO().DisplaySize - new Vector2(2, 26));
        ImGui.SetNextWindowPos(new Vector2(1, 25));
        if (ImGui.Begin($"##{ID}", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.AlwaysVerticalScrollbar)) {
            if (RootNode == null) {
                ImGui.Text(
                    "Welcome to the ImGui editor for TheAirBlow's Stupid Binary Format!\n" +
                    "To perform an action on a tree node, right-click on it - it will bring up a context menu.\n" +
                    $"NOTE: This editor will only work with SBF version {BinarySerializer.Version}.\n\n" +
                    "The icons may be confusing, so here's each one of them explained in order:\n" +
                    "1) Key entry type. Will only be shown if parent dictionary key is dynamic.\n" +
                    "2) Value entry type. Will only be shown if parent dictionary or array value is dynamic.\n" +
                    "3) Dictionary key entry type. Self-explanatory.\n" +
                    "4) Dictionary or array value entry type. Self-explanatory.\n\n" +
                    "Here's all icons with their corresponding entry types:");
                foreach (var str in Enum.GetNames<EntryType>()) {
                    Icons.Draw(str); ImGui.Text($" - {str}");
                }

                ImGui.End();
                return;
            }
            
            RenderNode(renderer, RootNode);
            ImGui.End();
        }
    }

    /// <summary>
    /// Renders a tree node
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    /// <param name="node">Tree Node</param>
    private void RenderNode(ImGuiRenderer renderer, TreeNode node) {
        var flags = ImGuiTreeNodeFlags.SpanFullWidth;
        if (node.NodeValueType is not EntryType.Dictionary and not EntryType.Array)
            flags |= ImGuiTreeNodeFlags.Leaf;
        var renderInner = ImGui.TreeNodeEx(node.ID, flags, "");
        if (ImGui.BeginPopupContextItem()) {
            if (node.NodeType != NodeType.ArrayContents && ImGui.MenuItem("Change type"))
                renderer.OpenWindow(new ChangeTypeWindow(node));
            
            if (node.NodeValueType == EntryType.Dictionary
                && ImGui.MenuItem("Add item"))
                renderer.OpenWindow(new AddItemWindow(node));
            
            if (node.NodeValueType == EntryType.Array
                && ImGui.MenuItem("Resize array"))
                renderer.OpenWindow(new ResizeArrayWindow(node));
            
            if (node.Parent != null 
                && node.NodeType != NodeType.ArrayContents
                && ImGui.MenuItem("Delete item"))
                node.Parent.Remove(node);
            
            if (ImGui.MenuItem("Edit item"))
                renderer.OpenWindow(new EditItemWindow(node));
            
            ImGui.EndPopup();
        }
        
        ImGui.SameLine(0, 2);
        if (node.Parent?.KeyType == EntryType.Dynamic) Icons.Draw(node.NodeKeyType);
        if (node.Parent == null || node.Parent.ValueType == EntryType.Dynamic) Icons.Draw(node.NodeValueType);
        if (node.KeyType.HasValue) Icons.Draw(node.KeyType.Value);
        if (node.ValueType.HasValue) Icons.Draw(node.ValueType.Value);
        switch (node.NodeValueType) {
            case EntryType.Array: {
                ImGui.Text($"{node.NodeKey} ({((Array)node.NodeValue).Length} elements)");
                if (renderInner) foreach (var child in node.Children.ToList()) RenderNode(renderer, child);
                break;
            }
            case EntryType.Dictionary: {
                ImGui.Text($"{node.NodeKey} ({((IDictionary)node.NodeValue).Count} elements)");
                if (renderInner) foreach (var child in node.Children.ToList()) RenderNode(renderer, child);
                break;
            }
            default:
                ImGui.Text($"{node.NodeKey}: {node.NodeValue}");
                break;
        }
        
        if (renderInner) ImGui.TreePop();
    }
}