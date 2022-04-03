using Dalamud.Game.ClientState.Aetherytes;
using Dalamud.Game.ClientState.Keys;
using Dalamud.IoC;
using ImGuiNET;
using System.Linq;

class Script
{
    private bool _showAetheryteList;

    [PluginService][RequiredVersion("1.0")] public static KeyState Keys { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static AetheryteList AetheryteList { get; private set; } = null!;

    public void Update()
    {
        _showAetheryteList = Keys[VirtualKey.F5];
    }

    public void Draw()
    {
        if (!_showAetheryteList)
            return;

        if (ImGui.Begin("Aetheryte List"))
        {
            if (ImGui.BeginTable("aetheryte_list_table", 2, ImGuiTableFlags.Borders))
            {
                ImGui.TableSetupColumn("Aetheryte");
                ImGui.TableSetupColumn("Cost");
                ImGui.TableHeadersRow();

                foreach (var aetheryte in AetheryteList.OrderByDescending(a => a.GilCost))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text(aetheryte.AetheryteData.GameData?.PlaceName.Value?.Name.ToString());
                    ImGui.TableNextColumn();
                    ImGui.Text(aetheryte.GilCost.ToString());
                }
                ImGui.EndTable();
            }
        }
        ImGui.End();
    }
}