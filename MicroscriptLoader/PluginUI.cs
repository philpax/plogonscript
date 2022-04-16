using System;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MicroscriptLoader;

internal class PluginUI : IDisposable
{
    private readonly Configuration _configuration;
    private readonly ScriptManager _scriptManager;
    private readonly WindowSystem _windowSystem = new("MicroscriptLoader.Windows");

    public PrimaryWindow PrimaryWindow { get; }

    public PluginUI(ScriptManager scriptManager, Configuration configuration)
    {
        _scriptManager = scriptManager;
        _configuration = configuration;
        PrimaryWindow = new PrimaryWindow(_scriptManager, _configuration);
        _windowSystem.AddWindow(PrimaryWindow);
    }

    public void Dispose()
    {
    }

    public void Draw()
    {
        _windowSystem.Draw();
    }
}