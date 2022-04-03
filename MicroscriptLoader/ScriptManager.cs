using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dalamud.Game;
using Dalamud.Plugin;

namespace MicroscriptLoader;

public class ScriptManager : IDisposable
{
    private readonly Configuration _configuration;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly string _scriptsPath;
    private readonly FileSystemWatcher _watcher;
    private readonly HashSet<string> _pendingLoads = new();
    private bool _pendingResync;

    public ScriptManager(DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
        _scriptsPath = Path.Combine(_pluginInterface.AssemblyLocation.Directory?.FullName!, "scripts");

        Directory.CreateDirectory(_scriptsPath);
        Resync();

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

        _watcher.Filter = "*.cs";
        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;

        foreach (var (key, _) in _configuration.AutoloadedScripts.Where(p => p.Value))
            if (Scripts.TryGetValue(key, out var script))
                script.Load();
    }

    public Dictionary<string, Script> Scripts { get; } = new();

    public void Dispose()
    {
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
        var scriptsOnDisk = Directory.EnumerateFiles(_scriptsPath, "*.cs").Select(a => Path.GetFileName(a)).ToHashSet();
        var scriptsHere = Scripts.Keys.ToHashSet();

        var scriptsToAdd = new HashSet<string>(scriptsOnDisk.AsEnumerable());
        scriptsToAdd.ExceptWith(scriptsHere.AsEnumerable());

        var scriptsToRemove = new HashSet<string>(scriptsHere.AsEnumerable());
        scriptsToRemove.ExceptWith(scriptsOnDisk.AsEnumerable());
        foreach (var scriptName in scriptsToAdd)
        {
            var script = new Script(Path.Combine(_scriptsPath, scriptName), _pluginInterface, _configuration);
            Scripts.Add(scriptName, script);
        }

        foreach (var scriptName in scriptsToRemove)
        {
            Scripts.Remove(scriptName, out var script);
            if (script != null) script.Dispose();
        }
    }

    public void OpenFolder()
    {
        Utils.OpenFolderInExplorer(_scriptsPath);
    }

    public void Draw()
    {
        foreach (var script in Scripts.Values)
            script.Call("Draw");
    }

    public void Update(Framework framework)
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

        foreach (var script in Scripts.Values)
            script.Call("Update");
    }
}