using System;
using Dalamud.Interface.Windowing;

namespace PlogonScript.UI;

internal class Main
{
    private readonly ScriptManager _scriptManager;
    private readonly WindowSystem _windowSystem = new("PlogonScript.Windows");

    public Main(ScriptManager scriptManager)
    {
        _scriptManager = scriptManager;

        var newScriptWindow = new NewScriptWindow(_scriptManager);
        _windowSystem.AddWindow(newScriptWindow);

        PrimaryWindow = new PrimaryWindow(_scriptManager, newScriptWindow);
        _windowSystem.AddWindow(PrimaryWindow);
    }

    public PrimaryWindow PrimaryWindow { get; }

    public void Draw()
    {
        _windowSystem.Draw();
    }
}