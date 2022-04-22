using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using PlogonScript.Script;

namespace PlogonScript.UI;

internal class NewScriptWindow : Window
{
    private readonly ScriptManager _scriptManager;
    private string _author = string.Empty;
    private string _filenameOverride = string.Empty;
    private string _name = string.Empty;
    private Dictionary<Event, bool> _generateEventHandlers = new();

    public NewScriptWindow(ScriptManager scriptManager) : base("New Script")
    {
        _scriptManager = scriptManager;

        Size = new Vector2(400, 400);
        SizeCondition = ImGuiCond.FirstUseEver;
        IsOpen = false;
    }

    private string GeneratedFilename => $"{Sanitise(_author, "NoAuthor")}-{Sanitise(_name, "NoName")}.js";
    private string Filename => _filenameOverride.IsNullOrEmpty() ? GeneratedFilename : _filenameOverride;

    private bool Valid => Filename.Length > 0 &&
                          Filename.EndsWith(".js") &&
                          Filename == Path.GetFileName(Filename) &&
                          Filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                          _name.Length > 0 &&
                          _author.Length > 0;

    private string Sanitise(string str, string def)
    {
        return !str.IsNullOrEmpty() ? string.Join('_', str.Split(' ')) : def;
    }

    public void OpenWithNewState()
    {
        _filenameOverride = _name = _author = "";
        _generateEventHandlers = Events.AllEvents.ToDictionary(evt => evt, _ => false);
        IsOpen = true;
    }

    public override void Draw()
    {
        ImGui.PushItemWidth(-float.Epsilon);

        ImGui.Text("Author:");
        ImGui.InputText("##Author", ref _author, 32);
        ImGui.Text("Name:");
        ImGui.InputText("##Name", ref _name, 32);

        var filename = Filename;
        ImGui.Text("Filename:");
        ImGui.InputText("##Filename", ref filename, 32);
        _filenameOverride = filename == GeneratedFilename ? "" : filename;

        ImGui.Separator();

        const float ButtonSize = 32.0f;
        ImGui.Text("Event handlers to generate:");
        ImGui.BeginChild("EventList", new Vector2(-float.Epsilon, -ButtonSize - 4), true);
        foreach (var key in _generateEventHandlers.Keys)
        {
            bool check = _generateEventHandlers[key];
            ImGui.Checkbox(key.Name, ref check);
            _generateEventHandlers[key] = check;
        }

        ImGui.EndChild();

        ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * (Valid ? 1.0f : 0.5f));
        var button = ImGui.Button("Create", new Vector2(-1.0f, ButtonSize));
        ImGui.PopStyleVar();

        if (Valid && button)
        {
            _scriptManager.Create(Filename, _name, _author,
                _generateEventHandlers.Where(kv => kv.Value).Select(kv => kv.Key));
            IsOpen = false;
        }

        ImGui.PopItemWidth();
    }
}