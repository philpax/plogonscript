using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

namespace PlogonScript;

public static class GlobalEvents
{
    public static readonly GlobalEvent OnLoad = new("onLoad");
    public static readonly GlobalEvent OnUnload = new("onUnload");
    public static readonly GlobalEvent OnDraw = new("onDraw");
    public static readonly GlobalEvent OnUpdate = new("onUpdate");

    public static readonly GlobalEvent OnKeyUp = new("onKeyUp",
        new Dictionary<string, Type> {{"key", typeof(VirtualKey)}});

    public static readonly GlobalEvent OnChatMessageUnhandled = new("onChatMessageUnhandled",
        new Dictionary<string, Type>
        {
            {"type", typeof(XivChatType)}, {"senderId", typeof(uint)}, {"sender", typeof(SeString)},
            {"message", typeof(SeString)}
        });

    public static readonly GlobalEvent[] Events = {OnLoad, OnUnload, OnDraw, OnUpdate, OnChatMessageUnhandled};
}