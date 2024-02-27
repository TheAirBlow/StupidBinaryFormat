using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using SBF.Core;

namespace SBF.Editor.Windows;

/// <summary>
/// Change type window
/// </summary>
public class ChangeTypeWindow : GuiWindow {
    /// <summary>
    /// New node key entry type
    /// </summary>
    private EntryType _nodeKey => Enum.Parse<EntryType>(KeyTypes[_indexes[0]]);
    
    /// <summary>
    /// New node value entry type
    /// </summary>
    private EntryType _nodeValue => Enum.Parse<EntryType>(ValueTypes[_indexes[1]]);
    
    /// <summary>
    /// New key entry type
    /// </summary>
    private EntryType? _key => _indexes[2] == -1 ? null : Enum.Parse<EntryType>(AllTypes[_indexes[2]]);
    
    /// <summary>
    /// New value entry type
    /// </summary>
    private EntryType? _value => _indexes[3] == -1 ? null : Enum.Parse<EntryType>(AllTypes[_indexes[3]]);
    
    /// <summary>
    /// Choice indexes
    /// </summary>
    private readonly int[] _indexes = { 0, 0, -1, -1 };
    
    /// <summary>
    /// Array of legal node key entry types
    /// </summary>
    public static readonly string[] KeyTypes;
    
    /// <summary>
    /// Array of legal node value entry types
    /// </summary>
    public static readonly string[] ValueTypes;
    
    /// <summary>
    /// The entire list of entry types
    /// </summary>
    public static readonly string[] AllTypes;
    
    /// <summary>
    /// Tree node
    /// </summary>
    private readonly TreeNode _node;

    /// <summary>
    /// Initializes choices arrays
    /// </summary>
    static ChangeTypeWindow() {
        AllTypes = Enum.GetNames<EntryType>();
        KeyTypes = AllTypes.ToArray(); 
        ValueTypes = AllTypes.ToArray(); 
        Array.Resize(ref KeyTypes,
            KeyTypes.Length - 3);
        Array.Resize(ref ValueTypes,
            ValueTypes.Length - 1);
    }
    
    /// <summary>
    /// Creates a new change type window
    /// </summary>
    /// <param name="node">Tree node</param>
    public ChangeTypeWindow(TreeNode node) {
        _node = node;
        _indexes[0] = Array.IndexOf(KeyTypes, _node.NodeKeyType.ToString());
        _indexes[1] = Array.IndexOf(ValueTypes, _node.NodeValueType.ToString());
        if (_node.KeyType != null) _indexes[2] = Array.IndexOf(AllTypes, _node.KeyType.ToString());
        if (_node.ValueType != null) _indexes[3] = Array.IndexOf(AllTypes, _node.ValueType.ToString());
    }
    
    /// <summary>
    /// Draws the change type window
    /// </summary>
    public override void DrawGUI(ImGuiRenderer renderer) {
        ImGui.OpenPopup($"Change type of {_node.NodeKey} ##{ID}");
        if (ImGui.BeginPopupModal($"Change type of {_node.NodeKey} ##{ID}", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize)) {
            ImGui.BeginDisabled(_node.Parent is not { KeyType: EntryType.Dynamic });
            ImGui.Combo("Node key type", ref _indexes[0], KeyTypes, KeyTypes.Length);
            ImGui.EndDisabled();
            ImGui.BeginDisabled(_node.Parent != null && _node.Parent.ValueType != EntryType.Dynamic);
            ImGui.Combo("Node value type", ref _indexes[1], ValueTypes, ValueTypes.Length);
            ImGui.EndDisabled();
            switch (_nodeValue) {
                case EntryType.Array:
                    ImGui.Combo("Array type", ref _indexes[3], AllTypes, AllTypes.Length);
                    break;
                case EntryType.Dictionary:
                    ImGui.Combo("Key type", ref _indexes[2], AllTypes, AllTypes.Length);
                    ImGui.Combo("Value type", ref _indexes[3], AllTypes, AllTypes.Length);
                    break;
            }
            var split = ImGui.GetWindowWidth() / 2;
            ImGui.BeginDisabled(
                (_node.NodeKeyType == _nodeKey && _node.NodeValueType == _nodeValue && _node.KeyType == _key && _node.ValueType == _value)
                || (_nodeKey == EntryType.Dictionary && (_key == null || _value == null))
                || (_nodeKey == EntryType.Array && _value == null));
            if (ImGui.Button("Apply", new Vector2(split - 12, 30))) {
                _node.ChangeKeyTo(_nodeKey);
                _node.ChangeValueTo(_nodeValue, 
                    valueType: _value, keyType: _key);
                IsOpen = false;
            }
            ImGui.EndDisabled(); ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(split - 12, 30)))
                IsOpen = false;
            ImGui.EndPopup();
        }
    }
}