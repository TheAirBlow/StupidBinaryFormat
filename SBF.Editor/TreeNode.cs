using System.Collections;
using SBF.Core;

namespace SBF.Editor;

/// <summary>
/// Tree node type
/// </summary>
public enum NodeType {
    /// <summary>
    /// A dictionary element
    /// </summary>
    DictionaryElement,
    
    /// <summary>
    /// An array element
    /// </summary>
    ArrayElement,
    
    /// <summary>
    /// Array's contents in string form
    /// </summary>
    ArrayContents,
    
    /// <summary>
    /// Root node
    /// </summary>
    RootNode
}

/// <summary>
/// A tree node
/// </summary>
public class TreeNode {
    /// <summary>
    /// Random tree node GUID
    /// </summary>
    public readonly string ID = Guid.NewGuid().ToString();
    
    /// <summary>
    /// List of node's children
    /// </summary>
    public readonly List<TreeNode> Children = new();
    
    /// <summary>
    /// Node's parent
    /// </summary>
    public readonly TreeNode? Parent;

    /// <summary>
    /// Node's key entry type
    /// </summary>
    public EntryType NodeKeyType;
    
    /// <summary>
    /// Node's value entry type
    /// </summary>
    public EntryType NodeValueType;
    
    /// <summary>
    /// Dictionary key entry type
    /// </summary>
    public EntryType? KeyType;
    
    /// <summary>
    /// Dictionary or array value entry type
    /// </summary>
    public EntryType? ValueType;
    
    /// <summary>
    /// Tree node type
    /// </summary>
    public NodeType NodeType;
    
    /// <summary>
    /// Node's value
    /// </summary>
    public object NodeValue;
    
    /// <summary>
    /// Node's key
    /// </summary>
    public object NodeKey;

    /// <summary>
    /// Creates a new tree node
    /// </summary>
    /// <param name="nodeKey">Key</param>
    /// <param name="nodeValue">Value</param>
    /// <param name="parent">Parent</param>
    /// <param name="nodeType">Node Type</param>
    public TreeNode(object nodeKey, object nodeValue, TreeNode? parent = null, NodeType nodeType = NodeType.RootNode) {
        NodeKey = nodeKey; NodeValue = nodeValue; Parent = parent; NodeType = nodeType;
        NodeKeyType = TypeHandler.Get(NodeKey.GetType());
        NodeValueType = TypeHandler.Get(NodeValue.GetType());
        GenerateChildren();
    }

    /// <summary>
    /// Resize array
    /// </summary>
    /// <param name="length">Length</param>
    public void ResizeArray(int length) {
        var old = (Array)NodeValue; var array = Array.CreateInstance(old.GetType().GetElementType()!, length);
        Array.Copy(old, array, Math.Min(old.Length, array.Length));
        for (var i = old.Length; i < array.Length; i++)
            array.SetValue(Utilities.GetDefault(ValueType!.Value), i);
        ChangeValueTo(EntryType.Array, array);
    }

    /// <summary>
    /// Removes child tree node from dictionary
    /// </summary>
    /// <param name="node">Tree Node</param>
    public void Remove(TreeNode node) {
        var dict = (IDictionary)NodeValue;
        dict.Remove(node.NodeKey);
        Children.Remove(node);
    }

    /// <summary>
    /// Change node's key entry type
    /// </summary>
    /// <param name="type">Entry Type</param>
    /// <param name="key">New Key</param>
    public void ChangeKeyTo(EntryType type, object? key = null) {
        key ??= Utilities.GetDefault(type);
        // It's safe to assume parent is a dictionary thanks
        // to the GUI being smart about disabling elements
        var dict = (IDictionary)Parent!.NodeValue;
        dict.Remove(NodeKey); NodeKeyType = type;
        NodeKey = dict[key] = key;
    }

    /// <summary>
    /// Change node's value entry type
    /// </summary>
    /// <param name="type">Entry Type</param>
    /// <param name="value">New Value</param>
    /// <param name="valueType">Value Entry Type</param>
    /// <param name="keyType">Key Entry Type</param>
    public void ChangeValueTo(EntryType type, object? value = null,
        EntryType? valueType = null, EntryType? keyType = null) {
        value ??= Utilities.GetDefault(type, valueType, keyType);
        ValueType = valueType; KeyType = keyType; 
        NodeValueType = type; NodeValue = value;
        GenerateChildren();
        switch (NodeType) {
            case NodeType.DictionaryElement:
                var dict = (IDictionary)Parent!.NodeValue;
                dict[NodeKey] = NodeValue;
                break;
            case NodeType.ArrayElement:
                var array = (Array)Parent!.NodeValue;
                var index = int.Parse(((string)NodeKey)[6..]);
                array.SetValue(NodeValue, index);
                break;
        }
    }

    /// <summary>
    /// Generates children nodes
    /// </summary>
    private void GenerateChildren() {
        Children.Clear();
        switch (NodeValueType) {
            case EntryType.Array:
                var arr = (Array)NodeValue;
                ValueType = TypeHandler.Get(arr.GetType().GetElementType()!);
                if (ValueType is not EntryType.Dictionary and not EntryType.Array and not EntryType.Dynamic) {
                    if (arr.Length == 0) {
                        Children.Add(new TreeNode("Contents", "[ ]", this, NodeType.ArrayContents));
                        break;
                    }
                    
                    Children.Add(new TreeNode("Contents", ValueType == EntryType.String 
                        ? $"[ {string.Join(", ", arr.Cast<string>().Select(x => $"\"{x.Replace("\"", "\\\"")}\""))} ]"
                        : $"[ {string.Join(", ", arr)} ]", this, NodeType.ArrayContents));
                    break;
                }
                
                for (var i = 0; i < arr.Length; i++)
                    Children.Add(new TreeNode($"Index {i}", arr.GetValue(i)!, this, NodeType.ArrayElement));
                break;
            case EntryType.Dictionary:
                var dict = (IDictionary)NodeValue;
                var args = dict.GetType().GetGenericArguments();
                KeyType = TypeHandler.Get(args[0]);
                ValueType = TypeHandler.Get(args[1]);
                var keys = dict.Keys.Cast<object>().ToList();
                var values = dict.Values.Cast<object>().ToList();
                for (var i = 0; i < dict.Count; i++)
                    Children.Add(new TreeNode(keys[i], values[i], this, NodeType.DictionaryElement));
                break;
        }
    }

    /// <summary>
    /// Change this node's key and value.
    /// </summary>
    /// <param name="key">New Key</param>
    /// <param name="value">New Value</param>
    public void Edit(string key, string value) {
        ChangeKeyTo(NodeKeyType, Utilities.ParseString(key, NodeKeyType));
        if (NodeValueType is EntryType.Array or EntryType.Dictionary) return;
        switch (NodeType) {
            case NodeType.DictionaryElement: 
            case NodeType.ArrayElement: {
                ChangeValueTo(NodeValueType, Utilities.ParseString(value, NodeValueType));
                break;
            }
            case NodeType.ArrayContents: {
                var list = Utilities.ParseArrayString(value, Parent!.ValueType!.Value);
                var array = Array.CreateInstance(Parent!.NodeValue.GetType().GetElementType()!, list.Count);
                for (var i = 0; i < list.Count; i++) array.SetValue(list[i], i);
                Parent.ChangeValueTo(EntryType.Array, array);
                break;
            }
        }
    }
}