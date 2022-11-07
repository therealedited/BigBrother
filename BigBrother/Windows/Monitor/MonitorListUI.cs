using BigBrother.Utils;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace BigBrother.Windows
{
    internal partial class MonitorWindow
    {

        private void DrawMonitoringListUI()
        {
            if (_plugin.TrackedPlayers is null) return;

            using var raii = new ImGuiRaii();
            var height = ImGui.GetContentRegionAvail().Y;
            height -= ImGui.GetStyle().ItemSpacing.Y;
            var width = ImGui.GetContentRegionAvail().X;

            if (!raii.Begin(() => ImGui.BeginListBox("##MonitorList", new Vector2(width, height)), ImGui.EndListBox))
            {
                return;
            }
            if (this._plugin.Configuration.TrackPeople)
            {
                foreach (KeyValuePair<IntPtr, GameObject> entry in _plugin.TrackedPlayers)
                {
                    AddEntry(entry.Value);
                }
            }
        }

        public bool IsCharacterIgnored(string name)
        =>
            name switch
            {
                "" => false,
                _ => _plugin.Configuration.ignorePlayers.FirstOrDefault(player => player.name.Contains(name)) != null,
            };
        
        private void AddEntry(GameObject? obj, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
        {
            if (obj == null) return;
            using var raii = new ImGuiRaii();

            var status = "";
            ImGui.BeginGroup();

            if (this._plugin.Configuration.MonitorMinions && obj.ObjectKind is ObjectKind.Companion)
            {
                status += $"{obj.YalmDistanceX} - {obj.YalmDistanceZ} | ";
                status += "M";
            }

            if (this._plugin.Configuration.MonitorWeapons && obj.ObjectKind is ObjectKind.Player)
            {
                status += $"{obj.YalmDistanceX} - {obj.YalmDistanceZ} | ";
                status += "W";
            }


            ImGui.PushStyleColor(ImGuiCol.Text, obj.ObjectKind == ObjectKind.Player ? _white : _red);
            ImGui.Selectable(obj.Name.TextValue, false, flags);

            var windowWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;
            ImGui.SameLine(windowWidth - ImGui.CalcTextSize(status).X);
            ImGui.TextUnformatted(status);
            ImGui.PopStyleColor();

            ImGui.EndGroup();

            //Thanks https://git.anna.lgbt/ascclemens/PeepingTom/src/branch/main/Peeping%20Tom/PluginUi.cs#L498
            var hovered = ImGui.IsItemHovered(ImGuiHoveredFlags.RectOnly);
            var leftClick = hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

            if (leftClick)
            {
                if (obj.ObjectKind == ObjectKind.Companion)
                {
                    var ownerID = Companion.GetCompanionOwnerID(obj.Address);
                    obj = _objects.SearchById(ownerID);
                }
                this._targetManager.Target = obj;

            }
        }

        private void BuildMonitoringList()
        {
            foreach (var obj in _objects)
            { 
                if (obj is null) continue;
                if (_plugin.TrackedPlayers.ContainsKey(obj.Address)) continue;
                if (IsCharacterIgnored(obj.Name.TextValue)) continue;
                if (Maths.CalculateEuclideanDistance(obj.YalmDistanceX, obj.YalmDistanceZ) > _plugin.Configuration.MonitorRange) continue;
                if (!(obj.ObjectKind == ObjectKind.Player && !Player.IsWeaponHidden((Character)obj)) && !(obj.ObjectKind == ObjectKind.Companion)) continue;

                if (_plugin.Configuration.PlaySounds && _windowSystem.GetWindow("Monitor")!.IsOpen)
                {
                    if(obj.ObjectKind == ObjectKind.Companion) _sounds.Play(_plugin.Configuration.SoundMinion);
                    if(obj.ObjectKind == ObjectKind.Player) _sounds.Play(_plugin.Configuration.SoundPlayer);
                }
                _plugin.TrackedPlayers.Add(obj.Address, obj);
            }
        }

        private void CleanMonitoringList()
        {
            bool valid = false;
            foreach (KeyValuePair<IntPtr, GameObject> entry in _plugin.TrackedPlayers)
            {
                var gameObject = entry.Value;
                foreach (var obj in _objects)
                {
                    if (IsStillValidTrack(obj.ObjectKind, obj, gameObject))
                    {
                        valid = true;
                    }
                }
                if (!valid) _plugin.TrackedPlayers.Remove(entry.Key);
            }
        }

        private bool IsStillValidTrack(ObjectKind objKind, GameObject gameObject1, GameObject gameObject2)
        =>
            objKind switch
            {
                ObjectKind.Companion => gameObject1.Address == gameObject2.Address,
                ObjectKind.Player => (gameObject1.Address == gameObject2.Address) && !Player.IsWeaponHidden((Character)gameObject2) && !IsCharacterIgnored(gameObject2.Name.TextValue),
                _ => false,
            };

    }
}
