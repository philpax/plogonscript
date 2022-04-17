using System;
using System.Collections.Generic;
using System.IO;
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

    public Script(string scriptPath, DalamudPluginInterface pluginInterface, Configuration configuration)
    {
        _pluginInterface = pluginInterface;
        _configuration = configuration;
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
            _engine.SetValue("PluginLog", TypeReference.CreateTypeReference(_engine, typeof(PluginLog)));
            _engine.SetValue("ImGui", TypeReference.CreateTypeReference(_engine, typeof(ImGui)));
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