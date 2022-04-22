using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dalamud;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Logging;
using Dalamud.Utility;
using ImGuiNET;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Newtonsoft.Json;

namespace PlogonScript.Script;

public readonly record struct ScriptMetadata(string Name, string Author)
{
    [JsonIgnore] public bool Valid => Name.Length > 0 && Author.Length > 0;
}

public class Script : IDisposable
{
    private readonly Configuration _configuration;
    private readonly List<Assembly> _whitelistAssemblies;
    private string _contents = string.Empty;
    private Engine? _engine;

    public Script(string path, Configuration configuration,
        List<Assembly> whitelistAssemblies, bool loadContents)
    {
        _configuration = configuration;
        _whitelistAssemblies = whitelistAssemblies;
        
        Path = path;
        if (loadContents)
            LoadContents();
    }

    public ScriptMetadata Metadata { get; set; } = new("", "");

    public string Filename => System.IO.Path.GetFileName(Path);
    public string DisplayName => Metadata.Name.Length > 0 ? Metadata.Name : Filename;

    public string Path { get; }

    public bool Loaded => _engine != null;

    public string Contents
    {
        get => _contents;
        set => _contents = value ?? throw new ArgumentNullException(nameof(value));
    }

    public void Dispose()
    {
        Unload(false);
        GC.SuppressFinalize(this);
    }

    public void LoadContents()
    {
        var contents = File.ReadAllText(Path);
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
        var contents = _contents;
        if (Metadata.Valid)
        {
            var metadata = JsonConvert.SerializeObject(Metadata);
            contents = $"//m:{metadata}\n{contents}";
        }

        File.WriteAllText(Path, contents);
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

            Initialize(_engine);

            _engine.Execute(_contents);
            GlobalEvents.OnLoad.Call(this);

            _configuration.AutoloadedScripts[Filename] = true;
            _configuration.Save();
        }
        catch
        {
            Unload(true);
            throw;
        }
    }

    private void Initialize(Engine engine)
    {
        engine.SetValue("Dalamud", new NamespaceReference(engine, "Dalamud"));
        engine.SetValue("PluginLog", TypeReference.CreateTypeReference(engine, typeof(PluginLog)));
        engine.SetValue("VirtualKey",
            TypeReference.CreateTypeReference(engine, typeof(VirtualKey)));

        // Inject all imgui types in
        foreach (var type in typeof(ImGui).Assembly.GetExportedTypes())
            engine.SetValue(type.Name, TypeReference.CreateTypeReference(engine, type));

        // Inject all of our services in
        Services.InjectIntoEngine(engine);

        // Provide an alternative console implementation
        engine.SetValue("console", new Bindings.Console(DisplayName));
    }

    public void Unload(bool disableAutoload)
    {
        if (disableAutoload)
        {
            _configuration.AutoloadedScripts[Filename] = false;
            _configuration.Save();
        }

        if (!Loaded) return;

        GlobalEvents.OnUnload.Call(this);
        _engine = null;
    }

    internal void CallGlobalFunction(string methodName, Dictionary<string, object>? arguments = null)
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