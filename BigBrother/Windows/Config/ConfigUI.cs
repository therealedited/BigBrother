using BigBrother.SeFunctions;
using BigBrother.Utils;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Enums;
using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace BigBrother.Windows
{
    internal partial class ConfigWindow
    {
        private int item_current_idx = 0;

        private List<SoundComboBoxItem> comboBoxSounds = new List<SoundComboBoxItem>()
        {
            new SoundComboBoxItem(Sounds.Sound01, "Sound01"),
            new SoundComboBoxItem(Sounds.Sound02, "Sound02"),
            new SoundComboBoxItem(Sounds.Sound03, "Sound03"),
            new SoundComboBoxItem(Sounds.Sound04, "Sound04"),
            new SoundComboBoxItem(Sounds.Sound05, "Sound05"),
            new SoundComboBoxItem(Sounds.Sound06, "Sound06"),
            new SoundComboBoxItem(Sounds.Sound07, "Sound07"),
            new SoundComboBoxItem(Sounds.Sound08, "Sound08"),
            new SoundComboBoxItem(Sounds.Sound09, "Sound09"),
            new SoundComboBoxItem(Sounds.Sound10, "Sound10"),
            new SoundComboBoxItem(Sounds.Sound11, "Sound11"),
            new SoundComboBoxItem(Sounds.Sound12, "Sound12"),
            new SoundComboBoxItem(Sounds.Sound13, "Sound13"),
            new SoundComboBoxItem(Sounds.Sound14, "Sound14"),
            new SoundComboBoxItem(Sounds.Sound15, "Sound15"),
            new SoundComboBoxItem(Sounds.Sound16, "Sound16"),
        };

        private string playerSound = "Sound01";
        private string minionSound = "Sound02";

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

            if (ImGui.BeginCombo("Player notification", _plugin.Configuration.SoundPlayer_s))
            {
                HandleComboBox(ObjectKind.Player);
            }
            if (ImGui.BeginCombo("Minion notification", _plugin.Configuration.SoundMinion_s))
            {
                HandleComboBox(ObjectKind.Companion);
            }
        }

        private void HandleComboBox(ObjectKind type)
        {
            for (int i = 0; i < comboBoxSounds.Count; i++)
            {
                bool is_selected = (item_current_idx == i);
                if (ImGui.Selectable(comboBoxSounds[i].name, is_selected))
                {
                    item_current_idx = i;
                    if (type is ObjectKind.Player)
                    {
                        playerSound = comboBoxSounds[i].name!;
                        _plugin.Configuration.SoundPlayer_s = comboBoxSounds[i].name!;
                        _plugin.Configuration.SoundPlayer = comboBoxSounds[i].sound;
                        _sounds.Play(_plugin.Configuration.SoundPlayer);
                        _plugin.Configuration.Save();
                    } else if (type is ObjectKind.Companion)
                    {
                        minionSound = comboBoxSounds[i].name!;
                        _plugin.Configuration.SoundMinion_s = comboBoxSounds[i].name!;
                        _plugin.Configuration.SoundMinion = comboBoxSounds[i].sound;
                        _sounds.Play(_plugin.Configuration.SoundMinion);
                        _plugin.Configuration.Save();
                    }
                    
                }

                if (is_selected && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                {
                    ImGui.SetItemDefaultFocus();
                }
            }
        }
    }
}
