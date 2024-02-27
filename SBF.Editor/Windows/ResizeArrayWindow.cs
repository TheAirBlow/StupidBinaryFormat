using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;

namespace SBF.Editor.Windows;

/// <summary>
/// Resize array window
/// </summary>
public class ResizeArrayWindow : GuiWindow {
    /// <summary>
    /// Tree node
    /// </summary>
    private readonly TreeNode _node;
    
    /// <summary>
    /// New length
    /// </summary>
    private int _newLength;
    
    /// <summary>
    /// Creates a new resize array window
    /// </summary>
    /// <param name="node">Tree Node</param>
    public ResizeArrayWindow(TreeNode node) {
        _node = node;
        var array = (Array)_node.NodeValue;
        _newLength = array.Length;
    }
    
    /// <summary>
    /// Draws the change type window
    /// </summary>
    public override void DrawGUI(ImGuiRenderer renderer) {
        ImGui.OpenPopup($"Resize {_node.NodeKey} ##{ID}");
        if (ImGui.BeginPopupModal($"Resize {_node.NodeKey} ##{ID}", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize)) {
            ImGui.InputInt("Length", ref _newLength);
            var split = ImGui.GetWindowWidth() / 2;
            if (ImGui.Button("Add", new Vector2(split - 12, 30))) {
                _node.ResizeArray(_newLength);
                IsOpen = false;
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(split - 12, 30)))
                IsOpen = false;
            ImGui.EndPopup();
        }
    }
}