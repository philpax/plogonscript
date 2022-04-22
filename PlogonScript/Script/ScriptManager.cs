using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dalamud.Plugin;

namespace PlogonScript.Script;

public class ScriptManager : IDisposable
{
    private readonly Configuration _configuration;
    private readonly HashSet<string> _pendingLoads = new();
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly string _scriptsPath;

    private readonly FileSystemWatcher _watcher;

    private bool _pendingResync;
    private readonly ScriptContainer _scriptContainer;

    public ScriptManager(DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;

        _scriptContainer = new ScriptContainer(configuration);

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

        _watcher.Filter = "*.js";
        _watcher.IncludeSubdirectories = false;
        _watcher.EnableRaisingEvents = true;

        foreach (var (key, _) in _configuration.AutoloadedScripts.Where(p => p.Value))
        {
            _scriptContainer.Get(key)?.Load();
        }
    }

    public IEnumerable<Script> Scripts => _scriptContainer.Scripts.Values;

    public Script? SelectedScript
    {
        get => _scriptContainer.Get(_configuration.SelectedScript);
        set => _configuration.SelectedScript = value?.Filename;
    }

    public void Dispose()
    {
        _watcher.Dispose();
        _scriptContainer.Dispose();
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
        var scriptsLoaded = _scriptContainer.Scripts.Keys.ToHashSet();

        var scriptsToAdd = new HashSet<string>(scriptsOnDisk.AsEnumerable());
        scriptsToAdd.ExceptWith(scriptsLoaded.AsEnumerable());
        foreach (var scriptName in scriptsToAdd)
            _scriptContainer.Add(MakeScript(scriptName, true));

        var scriptsToRemove = new HashSet<string>(scriptsLoaded.AsEnumerable());
        scriptsToRemove.ExceptWith(scriptsOnDisk.AsEnumerable());
        foreach (var script in scriptsToRemove.Select(_scriptContainer.Get).Where(a => a != null))
            _scriptContainer.Remove(script!);
    }

    private Script MakeScript(string scriptName, bool loadContents)
    {
        return _scriptContainer.MakeScript(Path.Combine(_scriptsPath, scriptName), loadContents);
    }

    public void OpenFolder()
    {
        Utils.OpenFolderInExplorer(_scriptsPath);
    }

    public void Update()
    {
        foreach (var load in _pendingLoads)
            if (_scriptContainer.Scripts.TryGetValue(load, out var script))
                script.LoadContents();
        _pendingLoads.Clear();

        if (_pendingResync)
        {
            Resync();
            _pendingResync = false;
        }

        if (SelectedScript == null || !_scriptContainer.Scripts.ContainsKey(SelectedScript.Filename))
            SelectedScript = Scripts.FirstOrDefault();

        _scriptContainer.Update();
    }

    public void Draw()
    {
        _scriptContainer.Draw();
    }

    public void Create(string filename, string name, string author, IEnumerable<Event> globalEvents)
    {
        var script = MakeScript(filename, false);
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
        _scriptContainer.Add(script);
        _configuration.SelectedScript = filename;
    }

    public void Delete(Script script)
    {
        var path = script.Path;
        _scriptContainer.Remove(script);
        File.Delete(path);
    }
}