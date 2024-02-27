using System.Numerics;
using System.Reflection;
using ImGuiNET;
using Raylib_ImGui;
using SBF.Core;

namespace SBF.Editor;

/// <summary>
/// Icons for all entry types
/// </summary>
public static class Icons {
    /// <summary>
    /// Entry type to binding dictionary
    /// </summary>
    private static readonly Dictionary<string, IntPtr> _icons = new();
    
    /// <summary>
    /// Load all icons
    /// </summary>
    static Icons() {
        var ass = Assembly.GetExecutingAssembly();
        foreach (var name in Enum.GetNames<EntryType>())
            _icons[name] = ass.GetEmbeddedResource($"{name}.png").LoadAsTexture(".png").CreateBinding();
    }
    
    /// <summary>
    /// Draws icon for specified entry type
    /// </summary>
    /// <param name="type">Entry Type</param>
    public static void Draw(string type) {
        ImGui.Image(_icons[type], new Vector2(16, 16));
        ImGui.SameLine(0, 2);
    }

    /// <summary>
    /// Draws icon for specified entry type
    /// </summary>
    /// <param name="type">Entry Type</param>
    public static void Draw(EntryType type)
        => Draw(type.ToString());
}