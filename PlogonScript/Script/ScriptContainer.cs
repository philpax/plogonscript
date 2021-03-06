using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace PlogonScript.Script;

public class ScriptContainer
{
    private readonly Dictionary<VirtualKey, bool> _prevKeyState;
    private readonly Configuration _configuration;
    private readonly Dictionary<string, Script> _scripts = new();

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

    public IReadOnlyDictionary<string, Script> Scripts => _scripts;

    public ScriptContainer(Configuration configuration)
    {
        // Load all of our whitelisted assemblies.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            if (assembly.GetName().Name != null && _whitelistAssemblyNames.Contains(assembly.GetName().Name!))
                _whitelistAssemblies.Add(assembly);

        _configuration = configuration;
        _prevKeyState = Services.KeyState.GetValidVirtualKeys().ToHashSet().ToDictionary(a => a, _ => false);

        Services.ChatGui.ChatMessageHandled += ChatGuiOnChatMessageHandled;
        Services.ChatGui.ChatMessageUnhandled += ChatGuiOnChatMessageUnhandled;
    }

    public void Dispose()
    {
        Services.ChatGui.ChatMessageUnhandled -= ChatGuiOnChatMessageUnhandled;
        Services.ChatGui.ChatMessageHandled -= ChatGuiOnChatMessageHandled;

        foreach (var (_, script) in Scripts) script.Dispose();
        _scripts.Clear();
    }

    public void Update()
    {
        UpdateKeyUp();
        CallEvent(Events.OnUpdate);
    }

    private void UpdateKeyUp()
    {
        foreach (var key in _prevKeyState.Keys)
        {
            var newState = Services.KeyState[key];
            var keyUp = !newState && _prevKeyState[key];
            _prevKeyState[key] = newState;

            if (keyUp)
                CallEvent(Events.OnKeyUp, ("key", key));
        }
    }

    public void Draw()
    {
        CallEvent(Events.OnDraw);
    }

    private void ChatGuiOnChatMessageHandled(XivChatType type, uint senderId, SeString sender, SeString message)
    {
        CallEvent(Events.OnChatMessageHandled, ("type", type), ("senderId", senderId), ("sender", sender),
            ("message", message));
    }

    private void ChatGuiOnChatMessageUnhandled(XivChatType type, uint senderId, SeString sender, SeString message)
    {
        CallEvent(Events.OnChatMessageUnhandled, ("type", type), ("senderId", senderId), ("sender", sender),
            ("message", message));
    }

    private void CallEvent(Event evt, params (string, object)[] arguments)
    {
        foreach (var script in Scripts.Values)
            evt.Call(script, arguments);
    }

    public Script MakeScript(string scriptPath, bool loadContents)
    {
        return new Script(scriptPath, _configuration, _whitelistAssemblies, loadContents);
    }

    public void Add(Script script)
    {
        _scripts.Add(script.Filename, script);
    }

    public void Remove(Script script)
    {
        _scripts.Remove(script.Filename);
        script.Dispose();
    }

    public Script? Get(string? filename)
    {
        if (filename == null)
            return null;

        return _scripts.ContainsKey(filename) ? _scripts[filename] : null;
    }
}