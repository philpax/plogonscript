using Jint;

namespace PlogonScript.Script.Bindings;

using S = PlogonScript.Services;

public class Services
{
    public static void Inject(Engine engine)
    {
        engine.SetValue("DataManager", S.DataManager);
        engine.SetValue("AetheryteList", S.AetheryteList);
        engine.SetValue("BuddyList", S.BuddyList);
        engine.SetValue("Condition", S.Condition);
        engine.SetValue("FateTable", S.FateTable);
        engine.SetValue("GamepadState", S.GamepadState);
        engine.SetValue("JobGauges", S.JobGauges);
        engine.SetValue("KeyState", S.KeyState);
        engine.SetValue("ObjectTable", S.ObjectTable);
        engine.SetValue("TargetManager", S.TargetManager);
        engine.SetValue("PartyList", S.PartyList);
        engine.SetValue("ClientState", S.ClientState);
        engine.SetValue("CommandManager", S.CommandManager);
        engine.SetValue("ContextMenu", S.ContextMenu);
        engine.SetValue("DtrBar", S.DtrBar);
        engine.SetValue("FlyTextGui", S.FlyTextGui);
        engine.SetValue("PartyFinderGui", S.PartyFinderGui);
        engine.SetValue("ToastGui", S.ToastGui);
        engine.SetValue("ChatGui", S.ChatGui);
        engine.SetValue("GameGui", S.GameGui);
        engine.SetValue("GameNetwork", S.GameNetwork);
        engine.SetValue("ChatHandlers", S.ChatHandlers);
    }
}