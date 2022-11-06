using Dalamud.Utility;
using ImGuiNET;

namespace BigBrother.Windows
{
    internal partial class ConfigWindow
    {
        private void DrawReportIssuesUI()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Report Issues"), ImGui.EndTabItem))
            {
                return;
            }
            if (ImGui.Button("Big Brother Discord"))
            {
                Util.OpenLink("https://discord.gg/qddbpeCWWw");
            }
        }
    }
}
