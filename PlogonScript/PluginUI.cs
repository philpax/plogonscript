using System;
using Dalamud.Interface.Windowing;

namespace PlogonScript;

internal class PluginUI : IDisposable
{
    private readonly Configuration _configuration;
    private readonly ScriptManager _scriptManager;
    private readonly WindowSystem _windowSystem = new("PlogonScript.Windows");

    public PluginUI(ScriptManager scriptManager, Configuration configuration)
    {
        _scriptManager = scriptManager;
        _configuration = configuration;

        var newScriptWindow = new NewScriptWindow(_scriptManager);
        _windowSystem.AddWindow(newScriptWindow);

        PrimaryWindow = new PrimaryWindow(_scriptManager, newScriptWindow);
        _windowSystem.AddWindow(PrimaryWindow);
    }

    public PrimaryWindow PrimaryWindow { get; }

    public void Dispose()
    {
    }

    public void Draw()
    {
        _windowSystem.Draw();
    }
}