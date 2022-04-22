using System;
using ImGuiNET;

namespace PlogonScript.Script.Bindings;

internal class UIHelpers
{
    public static void WithWindow(string name, Action body)
    {
        if (ImGui.Begin(name))
        {
            body();
        }
        ImGui.End();
    }
    
    public static void WithWindow(string name, ImGuiWindowFlags flags, Action body)
    {
        if (ImGui.Begin(name, flags))
        {
            body();
        }
        ImGui.End();
    }
}