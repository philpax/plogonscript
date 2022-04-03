using Dalamud.Logging;

class Script
{
    public void Load()
    {
        PluginLog.Information("Hello, world!");
    }

    public void Unload()
    {
        PluginLog.Information("Goodbye, world!");
    }
}