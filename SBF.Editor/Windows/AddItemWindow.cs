using System.Collections;
using System.Numerics;
using ImGuiNET;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using SBF.Core;

namespace SBF.Editor.Windows;

/// <summary>
/// Add item window
/// </summary>
public class AddItemWindow : GuiWindow {
    /// <summary>
    /// New node key entry type
    /// </summary>
    private EntryType _nodeKey => Enum.Parse<EntryType>(ChangeTypeWindow.KeyTypes[_indexes[0]]);
    
    /// <summary>
    /// New node value entry type
    /// </summary>
    private EntryType _nodeValue => Enum.Parse<EntryType>(ChangeTypeWindow.ValueTypes[_indexes[1]]);
    
    /// <summary>
    /// New key entry type
    /// </summary>
    private EntryType? _key => _indexes[2] == -1 ? null : Enum.Parse<EntryType>(ChangeTypeWindow.AllTypes[_indexes[2]]);
    
    /// <summary>
    /// New value entry type
    /// </summary>
    private EntryType? _value => _indexes[3] == -1 ? null : Enum.Parse<EntryType>(ChangeTypeWindow.AllTypes[_indexes[3]]);
    
    /// <summary>
    /// Choice indexes
    /// </summary>
    private readonly int[] _indexes = { 0, 0, -1, -1 };

    /// <summary>
    /// Node key string
    /// </summary>
    private string _keyString = "";
    
    /// <summary>
    /// Node value string
    /// </summary>
    private string _valueString = "";
    
    /// <summary>
    /// Tree node
    /// </summary>
    private readonly TreeNode _node;
    
    /// <summary>
    /// Creates a new add item window
    /// </summary>
    /// <param name="node">Tree Node</param>
    public AddItemWindow(TreeNode node) {
        _node = node;
        _indexes[0] = Array.IndexOf(ChangeTypeWindow.KeyTypes,
            _node.KeyType == EntryType.Dynamic ? "String" : _node.KeyType.ToString());
        _indexes[1] = Array.IndexOf(ChangeTypeWindow.ValueTypes,
            _node.ValueType == EntryType.Dynamic ? "String" : _node.ValueType.ToString());
    }
    
    /// <summary>
    /// Draws the change type window
    /// </summary>
    public override void DrawGUI(ImGuiRenderer renderer) {
        ImGui.OpenPopup($"Add item to {_node.NodeKey} ##{ID}");
        if (ImGui.BeginPopupModal($"Add item to {_node.NodeKey} ##{ID}", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize)) {
            ImGui.BeginDisabled(_node.KeyType != EntryType.Dynamic);
            ImGui.Combo("Key Type", ref _indexes[0],
                ChangeTypeWindow.KeyTypes, ChangeTypeWindow.KeyTypes.Length);
            ImGui.EndDisabled();
            ImGui.BeginDisabled(_node.ValueType != EntryType.Dynamic);
            ImGui.Combo("Value Type", ref _indexes[1],
                ChangeTypeWindow.ValueTypes, ChangeTypeWindow.ValueTypes.Length);
            ImGui.EndDisabled();
            ImGui.InputText("Node Key", ref _keyString, 255);
            switch (_nodeValue) {
                case EntryType.Array:
                    ImGui.Combo("Array type", ref _indexes[3],
                        ChangeTypeWindow.AllTypes, ChangeTypeWindow.AllTypes.Length);
                    break;
                case EntryType.Dictionary:
                    ImGui.Combo("Key type", ref _indexes[2],
                        ChangeTypeWindow.AllTypes, ChangeTypeWindow.AllTypes.Length);
                    ImGui.Combo("Value type", ref _indexes[3],
                        ChangeTypeWindow.AllTypes, ChangeTypeWindow.AllTypes.Length);
                    break;
                default:
                    ImGui.InputText("Node Value", ref _valueString, 255);
                    break;
            }
            ImGui.BeginDisabled(
                (_nodeValue == EntryType.Dictionary && (_key == null || _value == null))
                || (_nodeValue == EntryType.Array && _value == null));
            var split = ImGui.GetWindowWidth() / 2;
            if (ImGui.Button("Add", new Vector2(split - 12, 30))) {
                try {
                    var key = Utilities.ParseString(_keyString, _nodeKey);
                    switch (_nodeValue) {
                        case EntryType.Array: {
                            var node = new TreeNode(key, "", _node, NodeType.DictionaryElement);
                            node.ChangeValueTo(EntryType.Array, valueType: _value!.Value);
                            var dict = (IDictionary)_node.NodeValue; 
                            dict.Add(key, node.NodeValue);
                            _node.Children.Add(node);
                            break;
                        }
                        case EntryType.Dictionary: {
                            var node = new TreeNode(key, "", _node, NodeType.DictionaryElement);
                            node.ChangeValueTo(EntryType.Dictionary, valueType: _value, keyType: _key);
                            var dict = (IDictionary)_node.NodeValue;
                            dict.Add(key, node.NodeValue);
                            _node.Children.Add(node);
                            break;
                        }
                        default: {
                            var value = Utilities.ParseString(_valueString, _nodeValue);
                            var node = new TreeNode(key, value, _node, NodeType.DictionaryElement);
                            var dict = (IDictionary)_node.NodeValue; 
                            dict.Add(key, value);
                            _node.Children.Add(node);
                            break;
                        }
                    }
                    
                    IsOpen = false;
                } catch (Exception e) { 
                    renderer.OpenWindow(new PopupWindow(
                        "Failed to add element", e.ToString()));
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