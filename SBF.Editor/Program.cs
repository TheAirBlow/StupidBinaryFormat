using System.Numerics;
using System.Reflection;
using ImGuiNET;
using NativeFileDialog.Extended;
using Raylib_CsLo;
using Raylib_ImGui;
using Raylib_ImGui.Windows;
using SBF.Editor.Windows;

var renderer = new ImGuiRenderer();
Raylib.SetTraceLogLevel(4);
Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
Raylib.InitWindow(1280, 720, "SBF File Editor");
renderer.SwitchContext();
renderer.RecreateFontTexture();
var tree = new TreeWindow();
renderer.OpenWindow(tree);
ImGui.PushStyleVar(ImGuiStyleVar.WindowTitleAlign,
    new Vector2(0.5f, 0.5f));
ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 6);
ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 12);
ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

ImFontPtr fontPointer;
unsafe {
    var font = Assembly.GetCallingAssembly()
        .GetEmbeddedResource("DisposableDroid.ttf");
    fixed (byte* p = font) fontPointer = 
        ImGui.GetIO().Fonts.AddFontFromMemoryTTF(
            (IntPtr)p, font.Length, 20,
            ImGuiNative.ImFontConfig_ImFontConfig(),
            ImGui.GetIO().Fonts.GetGlyphRangesDefault());
    
    renderer.RecreateFontTexture();
}

while (!Raylib.WindowShouldClose()) {
    renderer.Update(); Raylib.BeginDrawing(); ImGui.NewFrame();
    Raylib.ClearBackground(new Color(42, 44, 48, 255));
    ImGui.PushFont(fontPointer);
    ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(),
        ImGuiDockNodeFlags.PassthruCentralNode);
    if (ImGui.BeginMainMenuBar()) {
        if (ImGui.BeginMenu("Open")) {
            if (ImGui.MenuItem("Create empty"))
                tree.SafeCreateEmpty(renderer);
            
            if (ImGui.MenuItem("Open file")) {
                var path = (string?)NFD.OpenDialog(".",
                    new Dictionary<string, string> {
                        ["Stupid Binary File"] = "sbf"
                    });
                if (path != null)
                    tree.OpenFileSafe(renderer, path);
            }
            
            ImGui.BeginDisabled(tree.RootNode == null);
            if (ImGui.MenuItem("Save file"))
                renderer.OpenWindow(new SaveFileWindow(tree));              
            ImGui.EndDisabled();
            
            ImGui.EndMenu();
        }
        
        if (ImGui.BeginMenu("Help")) {
            if (ImGui.MenuItem("About Raylib-ImGui"))
                renderer.OpenWindow(new AboutWindow());
                
            ImGui.EndMenu();
        }
        
        ImGui.EndMainMenuBar();
    }
    
    renderer.DrawWindows();
    renderer.RenderImGui();
    Raylib.EndDrawing();
}

Raylib.CloseWindow();