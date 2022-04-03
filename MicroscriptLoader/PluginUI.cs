using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace MicroscriptLoader;

internal class PluginUI : IDisposable
{
    private readonly Configuration _configuration;
    private readonly ScriptManager _scriptManager;

    private bool _settingsVisible;

    public PluginUI(ScriptManager scriptManager, Configuration configuration)
    {
        _scriptManager = scriptManager;
        _configuration = configuration;
    }

    public bool SettingsVisible
    {
        get => _settingsVisible;
        set => _settingsVisible = value;
    }

    private string? SelectedScript
    {
        get => _configuration.SelectedScript;
        set => _configuration.SelectedScript = value;
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
        DrawSettingsWindow();
    }

    private void DrawSettingsWindow()
    {
        if (!SettingsVisible) return;

        if (ImGui.Begin("Microscript Loader", ref _settingsVisible))
        {
            if (ImGui.Button("Open Scripts Folder")) _scriptManager.OpenFolder();

            if (SelectedScript == null || !_scriptManager.Scripts.ContainsKey(SelectedScript))
                SelectedScript = _scriptManager.Scripts.Keys.FirstOrDefault();

            // Left
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
            ImGui.SameLine();

            // Right
            {
                Script? script = null;
                if (SelectedScript != null)
                    _scriptManager.Scripts.TryGetValue(SelectedScript, out script);

                ImGui.BeginChild("item view",
                    new Vector2(0, 0), true, ImGuiWindowFlags.MenuBar); // Leave room for 1 line below us
                if (script != null)
                {
                    if (ImGui.BeginMenuBar())
                    {
                        if (ImGui.MenuItem("Save", "CTRL+S"))
                            script.SaveContents();

                        bool originalAutoload = _configuration.AutoloadedScripts.GetValueOrDefault(script.Name);
                        bool imguiAutoload = originalAutoload;
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

                    var contents = script.Contents;
                    ImGui.InputTextMultiline("##source", ref contents, 16384,
                        new Vector2(-float.Epsilon, -float.Epsilon), ImGuiInputTextFlags.AllowTabInput);
                    script.Contents = contents;
                }

                ImGui.EndChild();
            }
        }
        ImGui.End();
    }
}