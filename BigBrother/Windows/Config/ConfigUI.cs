using ImGuiNET;

namespace BigBrother.Windows
{
    internal partial class ConfigWindow
    {
        private void DrawConfigUI()
        {
            using var raii = new ImGuiRaii();
            var trackPeople = this._plugin.Configuration.TrackPeople;
            var trackWeapons = this._plugin.Configuration.MonitorWeapons;
            var trackMinions = this._plugin.Configuration.MonitorMinions;
            var playSounds = this._plugin.Configuration.PlaySounds;

            if (!raii.Begin(() => ImGui.BeginTabItem("Config"), ImGui.EndTabItem))
            {
                return;
            }
            if (ImGui.Checkbox("Monitor Area", ref trackPeople))
            {
                this._plugin.Configuration.TrackPeople = trackPeople;
                this._plugin.Configuration.Save();
            }

            ImGui.Separator();

            if (ImGui.Checkbox("Monitor Minions", ref trackMinions))
            {
                this._plugin.Configuration.MonitorMinions = trackMinions;
                this._plugin.Configuration.Save();
            }

            if (ImGui.Checkbox("Monitor Weapons", ref trackWeapons))
            {
                this._plugin.Configuration.MonitorWeapons = trackWeapons;
                this._plugin.Configuration.Save();
            }

            if (ImGui.Checkbox("Play Sounds", ref playSounds))
            {
                this._plugin.Configuration.PlaySounds = playSounds;
                this._plugin.Configuration.Save();
            }
            if (ImGui.SliderInt("Monitor Radius", ref _monitorRange, 0, 100))
            {
                _plugin.Configuration.MonitorRange = _monitorRange;
            }
        }
    }
}
