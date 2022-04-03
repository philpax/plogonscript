using Dalamud.Logging;
using Dalamud.Plugin;

class Script
{
    public void Load(DalamudPluginInterface pluginInterface)
    {
        PluginLog.Information("Hello, world!");
    }

    public void Unload()
    {
        PluginLog.Information("Goodbye, world!");
    }
}