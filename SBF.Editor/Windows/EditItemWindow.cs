using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using SBF.Core;

namespace SBF.Editor.Windows;

/// <summary>
/// Edit item window
/// </summary>
public class EditItemWindow : GuiWindow {
    /// <summary>
    /// Tree node
    /// </summary>
    private readonly TreeNode _node;
    
    /// <summary>
    /// Node key string
    /// </summary>
    private string _keyString;
    
    /// <summary>
    /// Node value string
    /// </summary>
    private string _valueString;

    /// <summary>
    /// Creates a new add item window
    /// </summary>
    /// <param name="node">Tree Node</param>
    public EditItemWindow(TreeNode node) {
        _node = node; _keyString = _node.NodeKey.ToString()!;
        _valueString = _node.NodeValue.ToString()!;
    }
    
    /// <summary>
    /// Draws the edit item window
    /// </summary>
    public override void DrawGUI(ImGuiRenderer renderer) {
        ImGui.OpenPopup($"Edit {_node.NodeKey} ##{ID}");
        if (ImGui.BeginPopupModal($"Edit {_node.NodeKey} ##{ID}", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize)) {
            ImGui.BeginDisabled(_node.NodeType is NodeType.ArrayContents or NodeType.ArrayElement);
            ImGui.InputText("Node Key", ref _keyString, 255);
            ImGui.EndDisabled();
            ImGui.BeginDisabled(_node.NodeValueType is EntryType.Array or EntryType.Dictionary);
            ImGui.InputText("Node Value", ref _valueString, 255);
            ImGui.EndDisabled();
            ImGui.BeginDisabled(_node.NodeKey.ToString() == _keyString
                && _node.NodeValue.ToString() == _valueString);
            var split = ImGui.GetWindowWidth() / 2;
            if (ImGui.Button("Apply", new Vector2(split - 12, 30))) {
                try {
                    _node.Edit(_keyString, _valueString);
                    IsOpen = false;
                } catch (Exception e) { 
                    renderer.OpenWindow(new PopupWindow(
                        "Failed to edit item", e.ToString()));
                }
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(split - 12, 30)))
                IsOpen = false;
            ImGui.EndPopup();
        }
    }
}