using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using PlogonScript.Script;

namespace PlogonScript.UI;

internal class Main
{
    private readonly ScriptManager _scriptManager;
    private readonly WindowSystem _windowSystem = new("PlogonScript.Windows");
    private readonly List<(Script.Script, string, string)> _errorModals = new();
    private readonly List<string> _popupsToOpen = new();

    public Main(ScriptManager scriptManager)
    {
        _scriptManager = scriptManager;

        var newScriptWindow = new NewScriptWindow(_scriptManager);
        _windowSystem.AddWindow(newScriptWindow);

        PrimaryWindow = new PrimaryWindow(_scriptManager, newScriptWindow);
        _windowSystem.AddWindow(PrimaryWindow);
    }

    public PrimaryWindow PrimaryWindow { get; }

    private void AddError(Script.Script script, string title, string message)
    {
        _errorModals.Add((script, title, message));
        _popupsToOpen.Add(title);
    }

    public void Draw()
    {
        _windowSystem.Draw();

        foreach (var title in _popupsToOpen)
        {
            ImGui.OpenPopup(title);
        }
        _popupsToOpen.Clear();

        var i = 0;
        var removedModals = new List<int>();
        foreach (var (script, title, message) in _errorModals)
        {
            var pOpen = true;
            if (ImGui.BeginPopupModal(title, ref pOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(message);
                ImGui.Separator();

                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                    removedModals.Add(i);
                }

                ImGui.SetItemDefaultFocus();
                ImGui.SameLine();
                if (ImGui.Button("Unload", new Vector2(120, 0)))
                {
                    script.Unload(false);
                    ImGui.CloseCurrentPopup();
                    removedModals.Add(i);
                }

                ImGui.EndPopup();
            }

            i++;
        }

        foreach (var index in removedModals.AsEnumerable().Reverse())
        {
            _errorModals.RemoveAt(index);
        }
    }

    public void Update()
    {
        foreach (var script in _scriptManager.Scripts)
        {
            foreach (var (title, message) in script.DrainErrorsForDisplay())
            {
                AddError(script, title, message);
            }
        }
    }
}