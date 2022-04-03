﻿using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace MicroscriptLoader;

public sealed class Plugin : IDalamudPlugin
{
    private readonly Framework _framework;
    private const string commandName = "/pmsl";

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] CommandManager commandManager,
        [RequiredVersion("1.0")] Framework framework)
    {
        _framework = framework;
        PluginInterface = pluginInterface;
        CommandManager = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        ScriptManager = new ScriptManager(PluginInterface, Configuration);
        PluginUi = new PluginUI(ScriptManager, Configuration);

        CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the configuration for Microscript Loader."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        _framework.Update += Update;
    }

    private void Update(Framework framework)
    {
        ScriptManager.Update(framework);
    }

    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }

    private Configuration Configuration { get; }
    private PluginUI PluginUi { get; }

    private ScriptManager ScriptManager { get; }
    public string Name => "Microscript Loader";

    public void Dispose()
    {
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
        PluginInterface.UiBuilder.Draw -= DrawUI;

        ScriptManager.Dispose();
        PluginUi.Dispose();
        CommandManager.RemoveHandler(commandName);
    }

    private void OnCommand(string command, string args)
    {
        PluginUi.SettingsVisible = true;
    }

    private void DrawUI()
    {
        PluginUi.Draw();
        ScriptManager.Draw();
    }

    private void DrawConfigUI()
    {
        PluginUi.SettingsVisible = true;
    }
}