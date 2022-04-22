using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;

namespace PlogonScript;

public sealed class Plugin : IDalamudPlugin
{
    private const string commandName = "/pps";
    private readonly Framework _framework;

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
            HelpMessage = "Open the configuration for PlogonScript."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        _framework.Update += Update;
    }

    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }

    private Configuration Configuration { get; }
    private PluginUI PluginUi { get; }

    private ScriptManager ScriptManager { get; }
    public string Name => "PlogonScript";

    public void Dispose()
    {
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;
        PluginInterface.UiBuilder.Draw -= DrawUI;

        ScriptManager.Dispose();
        PluginUi.Dispose();
        CommandManager.RemoveHandler(commandName);
    }

    private void Update(Framework framework)
    {
        ScriptManager.Update();
    }

    private void OnCommand(string command, string args)
    {
        PluginUi.PrimaryWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        PluginUi.Draw();
        ScriptManager.Draw();
    }

    private void DrawConfigUI()
    {
        PluginUi.PrimaryWindow.IsOpen = true;
    }
}