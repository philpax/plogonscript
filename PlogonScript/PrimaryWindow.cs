using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PlogonScript;

internal class PrimaryWindow : Window
{
    private readonly Configuration _configuration;
    private readonly ScriptManager _scriptManager;

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

    public override void Update()
    {
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

        if (ImGui.MenuItem("Open Scripts Folder"))
            _scriptManager.OpenFolder();

        ImGui.EndMenuBar();
    }

    private void DrawLeftPane()
    {
        ImGui.BeginChild("left pane", new Vector2(220, 0), true);

        foreach (var script in _scriptManager.Scripts.Values)
        {
            bool loaded = script.Loaded;
            if (ImGui.Checkbox("", ref loaded))
            {
                if (loaded)
                    script.Load();
                else
                    script.Unload(true);
            }
            ImGui.SameLine();

            if (ImGui.Selectable(script.DisplayName, script.Filename == SelectedScriptName))
                SelectedScriptName = script.Filename;
        }

        ImGui.EndChild();
    }

    private void DrawRightPane()
    {
        ImGui.BeginChild("item view", new Vector2(0, 0), true, ImGuiWindowFlags.MenuBar);
        if (SelectedScript != null)
        {
            bool openDelete = false;
            if (ImGui.BeginMenuBar())
            {
                string name = SelectedScript.Metadata.Name, author = SelectedScript.Metadata.Author;
                ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X * 0.2f);
                ImGui.Text("Name");
                ImGui.SameLine();
                ImGui.InputText("##ScriptName", ref name, 32);
                ImGui.SameLine();
                ImGui.Text("Author");
                ImGui.SameLine();
                ImGui.InputText("##ScriptAuthor", ref author, 32);
                SelectedScript.Metadata = new ScriptMetadata(name, author);

                if (ImGui.MenuItem("Save", SelectedScript.Metadata.Valid))
                    SelectedScript.SaveContents();

                if (SelectedScript.Loaded)
                {
                    if (ImGui.MenuItem("Reload"))
                    {
                        SelectedScript.Unload(false);
                        SelectedScript.Load();
                    }

                    if (ImGui.MenuItem("Unload")) SelectedScript.Unload(true);
                }
                else
                {
                    if (ImGui.MenuItem("Load")) SelectedScript.Load();
                }

                if (ImGui.MenuItem("Delete"))
                    openDelete = true;

                ImGui.PopItemWidth();
            }
            ImGui.EndMenuBar();

            var contents = SelectedScript.Contents;
            ImGui.InputTextMultiline("##source", ref contents, 16384,
                new Vector2(-float.Epsilon, -float.Epsilon), ImGuiInputTextFlags.AllowTabInput);
            SelectedScript.Contents = contents;

            if (openDelete)
                ImGui.OpenPopup("Delete?");
            DrawDeleteModal(SelectedScript);
        }
        ImGui.EndChild();
    }

    private void DrawDeleteModal(Script script)
    {
        var pOpen = true;
        if (!ImGui.BeginPopupModal("Delete?", ref pOpen, ImGuiWindowFlags.AlwaysAutoResize)) return;
        ImGui.Text($"The script `{script.DisplayName}` will be deleted.\nThis operation cannot be undone!\n\n");
        ImGui.Separator();
            
        if (ImGui.Button("OK", new Vector2(120, 0)))
        {
            _scriptManager.Delete(script);
            ImGui.CloseCurrentPopup();
        }

        ImGui.SetItemDefaultFocus();
        ImGui.SameLine();
        if (ImGui.Button("Cancel", new Vector2(120, 0)))
        {
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }
}