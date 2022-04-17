using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud;
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
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Utility;
using ImGuiNET;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Newtonsoft.Json;

namespace PlogonScript;

public readonly record struct ScriptMetadata(string Name, string Author)
{
    [JsonIgnore]
    public bool Valid => Name.Length > 0 && Author.Length > 0;
}

public class Script : IDisposable
{
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly List<Assembly> _whitelistAssemblies;
    private string _contents = string.Empty;
    private Engine? _engine;

    public Script(string scriptPath, DalamudPluginInterface pluginInterface, Configuration configuration,
        List<Assembly> whitelistAssemblies)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
        _whitelistAssemblies = whitelistAssemblies;
        _pluginInterface.Inject(this);

        ScriptPath = scriptPath;
        LoadContents();
    }

    public ScriptMetadata Metadata { get; set; } = new("", "");

    public string Filename => Path.GetFileName(ScriptPath);
    public string DisplayName => Metadata.Name.Length > 0 ? Metadata.Name : Filename;

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

    private string ScriptPath { get; }

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
        var contents = File.ReadAllText(ScriptPath);
        var lines = contents.Split('\n');
        if (lines.FirstOrDefault()!.StartsWith("//m:"))
        {
            Metadata = JsonConvert.DeserializeObject<ScriptMetadata>(lines.First().Replace("//m:", ""));
            lines = lines.Skip(1).ToArray();
        }

        _contents = string.Join("\n", lines);
    }

    public void SaveContents()
    {
        if (_contents.Length == 0)
            return;

        var contents = _contents;
        if (Metadata.Valid)
        {
            var metadata = JsonConvert.SerializeObject(Metadata);
            contents = $"//m:{metadata}\n{contents}";
        }

        File.WriteAllText(ScriptPath, contents);
    }

    public void Load()
    {
        try
        {
            if (_contents.IsNullOrEmpty())
                LoadContents();
            SaveContents();

            _engine = new Engine(options =>
            {
                options.AllowClr(typeof(EntryPoint).Assembly);
                options.CatchClrExceptions();
                options.Strict();
                options.Interop.AllowGetType = false;
                options.Interop.AllowSystemReflection = false;
                options.Interop.AllowedAssemblies = _whitelistAssemblies;
            });
            _engine.SetValue("Dalamud", new NamespaceReference(_engine, "Dalamud"));
            _engine.SetValue("PluginLog", TypeReference.CreateTypeReference(_engine, typeof(PluginLog)));
            _engine.SetValue("VirtualKey",
                TypeReference.CreateTypeReference(_engine, typeof(VirtualKey)));

            // inject all imgui types in
            foreach (var type in typeof(ImGui).Assembly.GetExportedTypes())
                _engine.SetValue(type.Name, TypeReference.CreateTypeReference(_engine, type));

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