using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PlogonScript;

internal class PrimaryWindow : Window
{
    private readonly ScriptManager _scriptManager;
    private readonly Configuration _configuration;

    public PrimaryWindow(ScriptManager scriptManager, Configuration configuration) : base("PlogonScript")
    {
        _scriptManager = scriptManager;
        _configuration = configuration;

        Size = new Vector2(1200, 675);
        SizeCondition = ImGuiCond.FirstUseEver;
        Flags |= ImGuiWindowFlags.MenuBar;
    }

    private string? SelectedScriptName
    {
        get => _configuration.SelectedScript;
        set => _configuration.SelectedScript = value;
    }

    private Script? SelectedScript
    {
        get
        {
            if (SelectedScriptName == null) return null;

            _scriptManager.Scripts.TryGetValue(SelectedScriptName, out var script);
            return script;
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();

        if (SelectedScriptName == null || !_scriptManager.Scripts.ContainsKey(SelectedScriptName))
            SelectedScriptName = _scriptManager.Scripts.Keys.FirstOrDefault();
    }

    public override void Draw()
    {
        DrawMenuBar();
        DrawLeftPane();
        ImGui.SameLine();
        DrawRightPane();
    }

    private void DrawMenuBar()
    {
        if (!ImGui.BeginMenuBar()) return;

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Open Scripts Folder"))
                _scriptManager.OpenFolder();

            if (SelectedScript != null)
            {
                if (ImGui.MenuItem("Save", "CTRL+S"))
                    SelectedScript.SaveContents();
            }

            ImGui.EndMenu();
        }

        if (SelectedScript != null && ImGui.BeginMenu("Script"))
        {
            var originalAutoload = _configuration.AutoloadedScripts.GetValueOrDefault(SelectedScript.Name);
            var imguiAutoload = originalAutoload;
            ImGui.Checkbox("Autoload", ref imguiAutoload);
            if (imguiAutoload != originalAutoload)
            {
                _configuration.AutoloadedScripts[SelectedScript.Name] = imguiAutoload;
                _configuration.Save();
            }

            if (SelectedScript.Loaded)
            {
                if (ImGui.MenuItem("Reload Script"))
                {
                    SelectedScript.Unload();
                    SelectedScript.Load();
                }

                if (ImGui.MenuItem("Unload Script"))
                {
                    SelectedScript.Unload();
                }
            }
            else
            {
                if (ImGui.MenuItem("Load Script"))
                {
                    SelectedScript.Load();
                }
            }

            ImGui.EndMenu();
        }

        ImGui.EndMenuBar();
    }

    private void DrawLeftPane()
    {
        ImGui.BeginChild("left pane", new Vector2(150, 0), true);

        foreach (var script in _scriptManager.Scripts.Values)
        {
            if (ImGui.Selectable(script.Name, script.Name == SelectedScriptName))
                SelectedScriptName = script.Name;
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
        ImGui.BeginChild("item view", new Vector2(0, 0), true);
        DrawScript();
        ImGui.EndChild();
    }

    private void DrawScript()
    {
        if (SelectedScript == null) return;

        var contents = SelectedScript.Contents;
        ImGui.InputTextMultiline("##source", ref contents, 16384,
            new Vector2(-float.Epsilon, -float.Epsilon), ImGuiInputTextFlags.AllowTabInput);
        SelectedScript.Contents = contents;
    }
}