using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using PlogonScript.Script;
using PlogonScript.UI;

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

        PluginInterface.Create<Services>();

        ScriptManager = new ScriptManager(PluginInterface, Configuration);
        Main = new Main(ScriptManager);

        CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the configuration for PlogonScript."
        });

        PluginInterface.UiBuilder.Draw += Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUI;

        _framework.Update += Update;
    }

    private DalamudPluginInterface PluginInterface { get; }
    private CommandManager CommandManager { get; }

    private Configuration Configuration { get; }
    private Main Main { get; }

    private ScriptManager ScriptManager { get; }
    public string Name => "PlogonScript";

    public void Dispose()
    {
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUI;
        PluginInterface.UiBuilder.Draw -= Draw;

        ScriptManager.Dispose();
        CommandManager.RemoveHandler(commandName);
    }

    private void Update(Framework framework)
    {
        ScriptManager.Update();
        Main.Update();
    }

    private void OnCommand(string command, string args)
    {
        Main.PrimaryWindow.IsOpen = true;
    }

    private void Draw()
    {
        ScriptManager.Draw();
        Main.Draw();
    }

    private void OpenConfigUI()
    {
        Main.PrimaryWindow.IsOpen = true;
    }
}