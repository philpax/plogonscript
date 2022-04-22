using System.Linq;
using Dalamud.Logging;
using Jint.Native;

namespace PlogonScript.Script.Bindings;

internal class Console
{
    private readonly string _displayName;

    public Console(string displayName)
    {
        _displayName = displayName;
    }

    private string ArgsToString(JsValue[] args)
    {
        // todo: improve
        return string.Join(" ", args.Select(a => a.ToString()));
    }

    public void Log(params JsValue[] args)
    {
        PluginLog.Information("{0:l}: {1:l}", _displayName, ArgsToString(args));
    }

    public void Debug(params JsValue[] args)
    {
        PluginLog.Debug("{0:l}: {1:l}", _displayName, ArgsToString(args));
    }

    public void Warn(params JsValue[] args)
    {
        PluginLog.Warning("{0:l}: {1:l}", _displayName, ArgsToString(args));
    }

    public void Error(params JsValue[] args)
    {
        PluginLog.Error("{0:l}: {1:l}", _displayName, ArgsToString(args));
    }
}