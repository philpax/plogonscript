using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace PlogonScript;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public Dictionary<string, bool> AutoloadedScripts = new();

    // the below exist just to make saving less cumbersome

    [NonSerialized] private DalamudPluginInterface? pluginInterface;
    public string? SelectedScript = string.Empty;

    public int Version { get; set; } = 0;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        pluginInterface!.SavePluginConfig(this);
    }
}