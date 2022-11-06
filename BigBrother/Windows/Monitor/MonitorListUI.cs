using BigBrother.Utils;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BigBrother.Windows
{
    internal partial class MonitorWindow
    {
        private void DrawMonitoringListUI()
        {
            if (_players is null) return;
            if (_trackedPlayers is null) return;

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
                foreach (KeyValuePair<IntPtr, GameObject> entry in _trackedPlayers)
                {
                    AddEntry(entry.Value);
                }
            }
        }

        //https://github.com/Ottermandias/Glamourer/blob/dbdaaf1ca89bcfd614c167614ce8120fb86f9fc2/Glamourer/CharacterExtensions.cs#L62
        public static unsafe bool IsWeaponHidden(Character a)
           => (*((byte*)a.Address + WeaponHidden1) & IsWeaponHidden1)
            == IsWeaponHidden1
            && (*((byte*)a.Address + WeaponHidden2) & IsWeaponHidden2)
            == IsWeaponHidden2;

        public static unsafe uint getCompanionOwnerID(IntPtr companion)
        {
            FFXIVClientStructs.FFXIV.Client.Game.Character.Character* companionStruct = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)(void*)companion;
            return companionStruct->CompanionOwnerID;
        }


        //Why?
        public bool IsCharacterIgnored(string name)
        {
            var allIgnoredPlayers = new List<string>();

            foreach (Player p in _configuration.ignorePlayers)
            {
                allIgnoredPlayers.Add(p.name);
            }

            return allIgnoredPlayers.FirstOrDefault(stringToCheck => stringToCheck.Contains(name)) != null;
        }

        public int calculateEuclideanDistance(int x, int z)
        {
            return (int)Math.Sqrt(x * x + z * z);
        }
        private void AddEntry(GameObject? obj, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None) //This function was clearly written with no goal in mind. Do not look unless you like to see spaghetti code.
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
                    var ownerID = getCompanionOwnerID(obj.Address);
                    obj = _objects.SearchById(ownerID);
                }
                this._targetManager.Target = obj;

            }
        }

        private Dictionary<IntPtr, GameObject> BuildMonitoringList()
        {
            var _trackedPlayers = new Dictionary<IntPtr, GameObject>();


            foreach (var obj in _objects)
            {
                if (_trackedPlayers!.ContainsKey(obj.Address)) //Don't add if the object already exists in the list.
                {
                    continue;
                }

                if (IsCharacterIgnored(obj.Name.TextValue))
                {
                    continue;
                }

                if (calculateEuclideanDistance(obj.YalmDistanceX, obj.YalmDistanceZ) > _configuration.MonitorRange)
                {
                    continue;
                }


                if ((obj.ObjectKind == ObjectKind.Player && !IsWeaponHidden((Character)obj)) || obj.ObjectKind == ObjectKind.Companion)
                {
                    _trackedPlayers.Add(obj.Address, obj);
                }
            }
            return _trackedPlayers;
        }

        private Dictionary<IntPtr, GameObject> CleanMonitoringList()
        {
            var _trackedPlayers = new Dictionary<IntPtr, GameObject>();
            var currentTrackedPlayers = BuildMonitoringList();
            _trackedPlayers = currentTrackedPlayers.Intersect(_trackedPlayers).ToDictionary(s => s.Key, s => s.Value); //Leave the compiler to guess the types.
            return _trackedPlayers;
        }

        private void PrintTrackedObjectList()
        {
            foreach(KeyValuePair<IntPtr, GameObject> trackedPlayer in _trackedPlayers)
            {
                PluginLog.Information(trackedPlayer.Key.ToString() + " - " + trackedPlayer.Value.Name.TextValue + "\n");
            }
        }
    }
}
