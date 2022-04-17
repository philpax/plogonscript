using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using ImGuiNET;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;

namespace MicroscriptLoader;

public class Script : IDisposable
{
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;
    private string _contents = string.Empty;
    private Engine? _engine;

    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Data.DataManager DataManager { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Aetherytes.AetheryteList AetheryteList { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Buddy.BuddyList BuddyList { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Conditions.Condition Condition { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Fates.FateTable FateTable { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0.0")]
    public static Dalamud.Game.ClientState.GamePad.GamepadState GamepadState { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.JobGauge.JobGauges JobGauges { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Keys.KeyState KeyState { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Objects.ObjectTable ObjectTable { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Objects.TargetManager TargetManager { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.Party.PartyList PartyList { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ClientState.ClientState ClientState { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Command.CommandManager CommandManager { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.ContextMenus.ContextMenu ContextMenu { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.Dtr.DtrBar DtrBar { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.FlyText.FlyTextGui FlyTextGui { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.PartyFinder.PartyFinderGui PartyFinderGui { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.Toast.ToastGui ToastGui { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.ChatGui ChatGui { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Gui.GameGui GameGui { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.Network.GameNetwork GameNetwork { get; private set; } = null!;
    [PluginService]
    [RequiredVersion("1.0")]
    public static Dalamud.Game.ChatHandlers ChatHandlers { get; private set; } = null!;


    public Script(string scriptPath, DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
        _pluginInterface.Inject(this);

        ScriptPath = scriptPath;
        LoadContents();
    }

    private string ScriptPath { get; }

    public string Name => Path.GetFileName(ScriptPath);
    public bool Loaded => _engine != null;

    public string Contents
    {
        get => _contents;
        set => _contents = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void Dispose()
    {
        Unload();
        GC.SuppressFinalize(this);
    }

    public void LoadContents()
    {
        _contents = File.ReadAllText(ScriptPath);
    }

    public void SaveContents()
    {
        if (_contents.Length == 0)
            return;

        File.WriteAllText(ScriptPath, _contents);
    }

    public void Load()
    {
        try
        {
            LoadContents();

            _engine = new Engine(options =>
            {
                options.AllowClr(typeof(Dalamud.EntryPoint).Assembly);
                options.CatchClrExceptions(e => true);
                options.Strict();
            });
            _engine.SetValue("Dalamud", new NamespaceReference(_engine, "Dalamud"));
            _engine.SetValue("PluginLog", TypeReference.CreateTypeReference(_engine, typeof(PluginLog)));
            _engine.SetValue("VirtualKey", TypeReference.CreateTypeReference(_engine, typeof(Dalamud.Game.ClientState.Keys.VirtualKey)));

            // inject all imgui types in
            foreach (var type in typeof(ImGui).Assembly.GetExportedTypes())
            {
                _engine.SetValue(type.Name, TypeReference.CreateTypeReference(_engine, type));
            }

            // Inject all of our services in
            _engine.SetValue("DataManager", DataManager);
            _engine.SetValue("AetheryteList", AetheryteList);
            _engine.SetValue("BuddyList", BuddyList);
            _engine.SetValue("Condition", Condition);
            _engine.SetValue("FateTable", FateTable);
            _engine.SetValue("GamepadState", GamepadState);
            _engine.SetValue("JobGauges", JobGauges);
            _engine.SetValue("KeyState", KeyState);
            _engine.SetValue("ObjectTable", ObjectTable);
            _engine.SetValue("TargetManager", TargetManager);
            _engine.SetValue("PartyList", PartyList);
            _engine.SetValue("ClientState", ClientState);
            _engine.SetValue("CommandManager", CommandManager);
            _engine.SetValue("ContextMenu", ContextMenu);
            _engine.SetValue("DtrBar", DtrBar);
            _engine.SetValue("FlyTextGui", FlyTextGui);
            _engine.SetValue("PartyFinderGui", PartyFinderGui);
            _engine.SetValue("ToastGui", ToastGui);
            _engine.SetValue("ChatGui", ChatGui);
            _engine.SetValue("GameGui", GameGui);
            _engine.SetValue("GameNetwork", GameNetwork);
            _engine.SetValue("ChatHandlers", ChatHandlers);

            _engine.Execute(_contents);
            Call("onLoad");
        }
        catch
        {
            Unload();
            throw;
        }
    }

    public void Unload()
    {
        if (!Loaded) return;

        Call("onUnload");
        _engine = null;
    }

    public void Call(string methodName, Dictionary<string, JsValue>? arguments = null)
    {
        if (_engine == null)
            return;

        var method = _engine.GetValue(methodName);
        if (method == null || method.IsUndefined())
            return;

        try
        {
            method.Call(JsValue.FromObject(_engine, arguments));
        }
        catch (Exception e)
        {
            PluginLog.Error("error: {0}", e);
        }
    }
}