using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace PlogonScript;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public Dictionary<string, bool> AutoloadedScripts = new();
    public string? SelectedScript = string.Empty;

    [NonSerialized] private DalamudPluginInterface? pluginInterface;

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