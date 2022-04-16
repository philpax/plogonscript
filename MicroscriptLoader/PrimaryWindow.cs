using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MicroscriptLoader;

internal class PrimaryWindow : Window
{
    private readonly ScriptManager _scriptManager;
    private readonly Configuration _configuration;

    public PrimaryWindow(ScriptManager scriptManager, Configuration configuration) : base(
        "Microscript Loader Settings")
    {
        _scriptManager = scriptManager;
        _configuration = configuration;
        
        Size = new Vector2(1200, 675);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    private string? SelectedScript
    {
        get => _configuration.SelectedScript;
        set => _configuration.SelectedScript = value;
    }

    public override void Draw()
    {
        if (ImGui.Button("Open Scripts Folder")) _scriptManager.OpenFolder();

        if (SelectedScript == null || !_scriptManager.Scripts.ContainsKey(SelectedScript))
            SelectedScript = _scriptManager.Scripts.Keys.FirstOrDefault();

        DrawLeftPane();
        ImGui.SameLine();
        DrawRightPane();
    }

    private void DrawLeftPane()
    {
        ImGui.BeginChild("left pane", new Vector2(150, 0), true);

        foreach (var script in _scriptManager.Scripts.Values)
        {
            if (ImGui.Selectable(script.Name, script.Name == SelectedScript))
                SelectedScript = script.Name;
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button,
                script.Loaded
                    ? new Vector4(0.54f, 0.60f, 0.06f, 1.0f)
                    : new Vector4(0.74f, 0.08f, 0.31f, 1.0f));
            ImGui.SmallButton("  ");
            ImGui.PopStyleColor();
        }

        ImGui.EndChild();
    }

    private void DrawRightPane()
    {
        // Leave room for 1 line below us
        ImGui.BeginChild("item view",
            new Vector2(0, 0), true, ImGuiWindowFlags.MenuBar);
        DrawSelectedScript();
        ImGui.EndChild();
    }

    private void DrawSelectedScript()
    {
        Script? script = null;
        if (SelectedScript != null)
            _scriptManager.Scripts.TryGetValue(SelectedScript, out script);
        if (script == null) return;

        DrawMenuBar(script);
        var contents = script.Contents;
        ImGui.InputTextMultiline("##source", ref contents, 16384,
            new Vector2(-float.Epsilon, -float.Epsilon), ImGuiInputTextFlags.AllowTabInput);
        script.Contents = contents;
    }

    private void DrawMenuBar(Script script)
    {
        if (!ImGui.BeginMenuBar()) return;

        if (ImGui.MenuItem("Save", "CTRL+S"))
            script.SaveContents();

        var originalAutoload = _configuration.AutoloadedScripts.GetValueOrDefault(script.Name);
        var imguiAutoload = originalAutoload;
        ImGui.Checkbox("Autoload", ref imguiAutoload);
        if (imguiAutoload != originalAutoload)
        {
            _configuration.AutoloadedScripts[script.Name] = imguiAutoload;
            _configuration.Save();
        }

        if (script.Loaded)
        {
            if (ImGui.MenuItem("Reload Script"))
            {
                script.Unload();
                script.Load();
            }

            ImGui.SameLine();
            if (ImGui.MenuItem("Unload Script")) script.Unload();
        }
        else
        {
            if (ImGui.MenuItem("Load Script")) script.Load();
        }

        ImGui.EndMenuBar();
    }
}