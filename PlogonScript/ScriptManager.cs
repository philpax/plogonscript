using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin;

namespace PlogonScript;

public class ScriptManager : IDisposable
{
    private readonly Configuration _configuration;
    private readonly HashSet<string> _pendingLoads = new();
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly string _scriptsPath;

    private readonly FileSystemWatcher _watcher;

    private readonly List<Assembly> _whitelistAssemblies = new();

    private readonly HashSet<string> _whitelistAssemblyNames = new()
    {
        "Dalamud",
        "FFXIVClientStructs",
        "ImGui.NET",
        "ImGuiScene",
        "Lumina",
        "Lumina.Excel",
        "Newtonsoft.Json",
        "Serilog",
        "System.Collections",
        "System.Collections.Concurrent",
        "System.Collections.Immutable",
        "System.Collections.NonGeneric",
        "System.Collections.Specialized",
        "System.Data.Common",
        "System.Globalization",
        "System.Linq",
        "System.Linq.Expressions",
        "System.Numerics.Vectors",
        "System.Runtime",
        "System.Runtime.Extensions",
        "System.Text.Encoding.Extensions",
        "System.Text.Encodings.Web",
        "System.Text.Json",
        "System.Text.RegularExpressions"
    };

    private bool _pendingResync;
    private readonly Dictionary<VirtualKey, bool> _prevKeyState;

    public ScriptManager(DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
        _pluginInterface.Create<ScriptServices>();

        _prevKeyState = ScriptServices.KeyState.GetValidVirtualKeys().ToHashSet().ToDictionary(a => a, _ => false);

        _scriptsPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, "scripts");

        // Load all of our whitelisted assemblies.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            if (assembly.GetName().Name != null && _whitelistAssemblyNames.Contains(assembly.GetName().Name!))
                _whitelistAssemblies.Add(assembly);

        Directory.CreateDirectory(_scriptsPath);
        Resync();

        ScriptServices.ChatGui.ChatMessageUnhandled += ChatGuiOnChatMessageUnhandled;

        _watcher = new FileSystemWatcher(_scriptsPath);
        _watcher.NotifyFilter = NotifyFilters.Attributes
                                | NotifyFilters.CreationTime
                                | NotifyFilters.DirectoryName
                                | NotifyFilters.FileName
                                | NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.Security
                                | NotifyFilters.Size;

        _watcher.Changed += (_, e) => HandleFilesystemEvent(e);
        _watcher.Created += (_, e) => HandleFilesystemEvent(e);
        _watcher.Deleted += (_, e) => HandleFilesystemEvent(e);
        _watcher.Renamed += (_, e) => HandleFilesystemEvent(e);

        _watcher.Filter = "*.js";
        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;

        foreach (var (key, _) in _configuration.AutoloadedScripts.Where(p => p.Value))
            if (Scripts.TryGetValue(key, out var script))
                script.Load();
    }

    public string? SelectedScriptName
    {
        get => _configuration.SelectedScript;
        set => _configuration.SelectedScript = value;
    }

    public Dictionary<string, Script> Scripts { get; } = new();

    public void Dispose()
    {
        ScriptServices.ChatGui.ChatMessageUnhandled -= ChatGuiOnChatMessageUnhandled;

        _watcher.Dispose();

        foreach (var (_, script) in Scripts) script.Dispose();
        Scripts.Clear();
    }

    private void HandleFilesystemEvent(FileSystemEventArgs? fileSystemEventArgs)
    {
        if (fileSystemEventArgs != null && (fileSystemEventArgs.ChangeType & WatcherChangeTypes.Changed) != 0)
            _pendingLoads.Add(fileSystemEventArgs.Name!);

        _pendingResync = true;
    }

    private void Resync()
    {
        var scriptsOnDisk = Directory.EnumerateFiles(_scriptsPath, "*.js").Select(Path.GetFileName).Select(a => a!)
            .ToHashSet();
        var scriptsHere = Scripts.Keys.ToHashSet();

        var scriptsToAdd = new HashSet<string>(scriptsOnDisk.AsEnumerable());
        scriptsToAdd.ExceptWith(scriptsHere.AsEnumerable());

        var scriptsToRemove = new HashSet<string>(scriptsHere.AsEnumerable());
        scriptsToRemove.ExceptWith(scriptsOnDisk.AsEnumerable());
        foreach (var scriptName in scriptsToAdd) Scripts.Add(scriptName, CreateScript(scriptName, true));

        foreach (var scriptName in scriptsToRemove)
        {
            Scripts.Remove(scriptName, out var script);
            script?.Dispose();
        }
    }

    private Script CreateScript(string scriptName, bool loadContents)
    {
        var scriptPath = Path.Combine(_scriptsPath, scriptName);
        return new Script(scriptPath, _configuration,
            _whitelistAssemblies, loadContents);
    }

    public void OpenFolder()
    {
        Utils.OpenFolderInExplorer(_scriptsPath);
    }

    public void Draw()
    {
        CallEvent(GlobalEvents.OnDraw);
    }

    public void Update()
    {
        foreach (var load in _pendingLoads)
            if (Scripts.TryGetValue(load, out var script))
                script.LoadContents();
        _pendingLoads.Clear();

        if (_pendingResync)
        {
            Resync();
            _pendingResync = false;
        }

        // Update the KeyUp state.
        foreach (var key in _prevKeyState.Keys)
        {
            var newState = ScriptServices.KeyState[key];
            var keyUp = !newState && _prevKeyState[key];
            _prevKeyState[key] = newState;

            if (keyUp)
                CallEvent(GlobalEvents.OnKeyUp, new Dictionary<string, object> {{"key", key}});
        }

        CallEvent(GlobalEvents.OnUpdate);
    }

    private void ChatGuiOnChatMessageUnhandled(XivChatType type, uint senderId, SeString sender, SeString message)
    {
        CallEvent(GlobalEvents.OnChatMessageUnhandled, new Dictionary<string, object>
        {
            {"type", type}, {"senderId", senderId}, {"sender", sender}, {"message", message}
        });
    }

    public void Create(string filename, string name, string author, IEnumerable<GlobalEvent> globalEvents)
    {
        var script = CreateScript(filename, false);
        script.Metadata = new ScriptMetadata(name, author);
        foreach (var globalEvent in globalEvents)
        {
            var contentsBuilder = new StringBuilder();
            contentsBuilder.Append("function ");
            contentsBuilder.Append(globalEvent.Name);
            contentsBuilder.Append('(');
            if (globalEvent.Arguments.Count > 0)
            {
                contentsBuilder.Append('{');
                contentsBuilder.Append(string.Join(", ", globalEvent.Arguments.Keys));
                contentsBuilder.Append('}');
            }

            contentsBuilder.Append(") {\n}\n\n");
            script.Contents += contentsBuilder.ToString();
        }

        script.SaveContents();
        Scripts.Add(filename, script);
        SelectedScriptName = filename;
    }

    public void Delete(Script script)
    {
        var path = script.Path;
        Scripts.Remove(script.Filename);
        script.Dispose();
        File.Delete(path);
    }

    internal void CallEvent(GlobalEvent evt, Dictionary<string, object>? arguments = null)
    {
        foreach (var script in Scripts.Values)
            script.CallGlobalFunction(evt.Name, arguments);
    }
}