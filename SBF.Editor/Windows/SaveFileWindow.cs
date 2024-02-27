using System.Collections;
using System.Numerics;
using ImGuiNET;
using NativeFileDialog.Extended;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using SBF.Core;

namespace SBF.Editor.Windows;

public class SaveFileWindow : GuiWindow {
    /// <summary>
    /// Tree window instance
    /// </summary>
    private readonly TreeWindow _window;

    /// <summary>
    /// Is file compressed
    /// </summary>
    private bool _compressed;
    
    /// <summary>
    /// Save file path
    /// </summary>
    private string _path;

    /// <summary>
    /// Creates a new save file window
    /// </summary>
    /// <param name="window">Tree Window</param>
    public SaveFileWindow(TreeWindow window) {
        _window = window; _path = _window.Path;
    }
    
    /// <summary>
    /// Draw the GUI
    /// </summary>
    /// <param name="renderer">ImGui renderer</param>
    public override void DrawGUI(ImGuiRenderer renderer) {
        ImGui.OpenPopup($"Save file ##{ID}");
        if (ImGui.BeginPopupModal($"Save file ##{ID}", ref IsOpen, ImGuiWindowFlags.AlwaysAutoResize)) {
            ImGui.InputText("##path", ref _path, 255);
            if (ImGui.Button("Browse")) { 
                var path = (string?)NFD.SaveDialog(".", "filename.sbf",
                    new Dictionary<string, string> {
                        ["Stupid Binary File"] = "sbf"
                    });
                if (path != null) _path = path;
            }
            ImGui.SameLine();
            ImGui.Checkbox("GZip Compress", ref _compressed);
            var split = ImGui.GetWindowWidth() / 2;
            ImGui.BeginDisabled(false);
            if (ImGui.Button("Save", new Vector2(split - 12, 30))) {
                try {
                    using var file = new FileStream(_path, FileMode.Create, FileAccess.Write);
                    BinarySerializer.Serialize(file, _window.RootNode!.NodeValue, _compressed);
                    IsOpen = false;
                } catch (Exception e) {
                    renderer.OpenWindow(new PopupWindow("Failed to save file", e.ToString()));
                }
            }
            ImGui.EndDisabled(); ImGui.SameLine();
            if (ImGui.Button("Cancel", new Vector2(split - 12, 30)))
                IsOpen = false;
            ImGui.EndPopup();
        }
    }
}