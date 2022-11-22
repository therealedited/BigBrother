using BigBrother.Utils;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;

namespace BigBrother.Windows
{
    internal partial class ConfigWindow
    {
        private byte[] buffer = new byte[256];
        private void DrawIgnorePlayerUI()
        {
            using var raii = new ImGuiRaii();

            if (!raii.Begin(() => ImGui.BeginTabItem("Ignore List"), ImGui.EndTabItem))
            {
                return;
            }
            ImGui.Text("Add player to the ignore list");
            ImGui.InputText("##input", buffer, (uint)buffer.Length);
            ImGui.SameLine();
            if (ImGui.Button("+"))
            {
                var player = new Player(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length), ++Plugin.Configuration.ignoredPlayersNumber);
                Plugin.Configuration.ignorePlayers.Add(player);
                Plugin.Configuration.Save();
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }
            }
            ImGui.Separator();
            raii.Begin(() => ImGui.BeginTable("##IgnoredPlayersTable", 2), ImGui.EndTable);

            ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, 30);
            ImGui.TableSetupColumn("Player name", ImGuiTableColumnFlags.WidthFixed, ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X);
            ImGui.TableHeadersRow();
            
            for (var i = 0; i < Plugin.Configuration.ignorePlayers.Count; i++)
            {
                var player = (Player)Plugin.Configuration.ignorePlayers[i];
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.Button($"DEL##{i}"))
                {
                    Plugin.Configuration.ignorePlayers.RemoveAt(i--);
                    Plugin.Configuration.ignoredPlayersNumber -= 1;
                    Plugin.Configuration.Save();
                    continue;
                }
                ImGui.TableNextColumn();
                ImGui.Text(player.name);
            }
        } 
    }
}
