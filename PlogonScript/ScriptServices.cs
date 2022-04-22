using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.ClientState.Buddy;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.ClientState.GamePad;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.Game.Gui.ContextMenus;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.FlyText;
using Dalamud.Game.Gui.PartyFinder;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Network;
using Dalamud.IoC;
using Jint;

namespace PlogonScript;

public class ScriptServices
{
    [PluginService]
    [RequiredVersion("1.0")]
    public static DataManager DataManager { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static AetheryteList AetheryteList { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static BuddyList BuddyList { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static Condition Condition { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static FateTable FateTable { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0.0")]
    public static GamepadState GamepadState { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static JobGauges JobGauges { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static KeyState KeyState { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static ObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static TargetManager TargetManager { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static PartyList PartyList { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static ClientState ClientState { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static CommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static ContextMenu ContextMenu { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static DtrBar DtrBar { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static FlyTextGui FlyTextGui { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static PartyFinderGui PartyFinderGui { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static ToastGui ToastGui { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static ChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static GameGui GameGui { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static GameNetwork GameNetwork { get; private set; } = null!;

    [PluginService]
    [RequiredVersion("1.0")]
    public static ChatHandlers ChatHandlers { get; private set; } = null!;

    public static void InjectIntoEngine(Engine engine)
    {
        engine.SetValue("DataManager", DataManager);
        engine.SetValue("AetheryteList", AetheryteList);
        engine.SetValue("BuddyList", BuddyList);
        engine.SetValue("Condition", Condition);
        engine.SetValue("FateTable", FateTable);
        engine.SetValue("GamepadState", GamepadState);
        engine.SetValue("JobGauges", JobGauges);
        engine.SetValue("KeyState", KeyState);
        engine.SetValue("ObjectTable", ObjectTable);
        engine.SetValue("TargetManager", TargetManager);
        engine.SetValue("PartyList", PartyList);
        engine.SetValue("ClientState", ClientState);
        engine.SetValue("CommandManager", CommandManager);
        engine.SetValue("ContextMenu", ContextMenu);
        engine.SetValue("DtrBar", DtrBar);
        engine.SetValue("FlyTextGui", FlyTextGui);
        engine.SetValue("PartyFinderGui", PartyFinderGui);
        engine.SetValue("ToastGui", ToastGui);
        engine.SetValue("ChatGui", ChatGui);
        engine.SetValue("GameGui", GameGui);
        engine.SetValue("GameNetwork", GameNetwork);
        engine.SetValue("ChatHandlers", ChatHandlers);
    }
}