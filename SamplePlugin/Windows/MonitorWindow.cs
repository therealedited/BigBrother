using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text;
using Lumina.Data;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;

namespace BigBrother.Windows;

//Many thanks to ascclemens for her PeepingTom plugin.
public class MonitorWindow : Window, IDisposable
{
    private Configuration Configuration;

    private Dictionary<uint, GameObject>? _players = new Dictionary<uint, GameObject>();

    private Plugin _plugin;

    private const int WeaponHidden1 = 0x85F;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private const int WeaponHidden2 = 0x73C; //Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private const byte IsWeaponHidden1 = 0x01;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private const byte IsWeaponHidden2 = 0x02;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private ObjectTable _objects;
    private TargetManager _targetManager;


    public MonitorWindow(Plugin plugin, ObjectTable objects, TargetManager targetManager) : base(
        "Monitor")
    {
        this.Size = new Vector2(232, 300);
        this.SizeCondition = ImGuiCond.Once;
        this.Configuration = plugin.Configuration;
        this._plugin = plugin;
        _objects = objects;
        _targetManager = targetManager;
    }

    public void Dispose() {  
    }

    public override void Draw()
    {
        DrawList();
        
    }

    private void DrawList()
    {
        var height = ImGui.GetContentRegionAvail().Y;
        height -= ImGui.GetStyle().ItemSpacing.Y;
        var width = ImGui.GetContentRegionAvail().X;
        if (this.Configuration.CleaningStarted)
        {
            CleanMonitoringList();
        }
        BuildMonitoringList();
        if (ImGui.BeginListBox("###monitoring", new Vector2(width, height)))
        {
            if (this._plugin.Configuration.TrackPeople)
            {
                foreach (KeyValuePair<uint, GameObject> entry in _players)
                {
                    AddEntry(entry.Value);
                }
            }
            ImGui.EndListBox();
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

    private void AddEntry(GameObject? obj, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.BeginGroup();
        var status = "";

        if (this._plugin.Configuration.MonitorMinions)
        {
            if (obj.ObjectKind is ObjectKind.Companion)
            {
                status += $"{obj.YalmDistanceX} - {obj.YalmDistanceZ} | ";
                status += "M";
            }
        }

        if (this._plugin.Configuration.MonitorWeapons)
        {
            if (obj is Character character && obj.ObjectKind == ObjectKind.Player)
            {
                if (!IsWeaponHidden(character))
                {
                    status += $"{obj.YalmDistanceX} - {obj.YalmDistanceZ} | ";
                    status += "W";
                }
            }
        }

        if ((!status.Contains("M") && !(status.Contains("W"))))
        {
            return;
        }

        ImGui.Selectable(obj.Name.TextValue, false, flags);

        var windowWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;
        ImGui.SameLine(windowWidth - ImGui.CalcTextSize(status).X);

        ImGui.TextUnformatted(status);

        ImGui.EndGroup();

        //Thanks https://git.anna.lgbt/ascclemens/PeepingTom/src/branch/main/Peeping%20Tom/PluginUi.cs#L498
        var hovered = ImGui.IsItemHovered(ImGuiHoveredFlags.RectOnly);
        var leftClick = hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Left);
        var rightClick = hovered && ImGui.IsMouseClicked(ImGuiMouseButton.Right);

        if (leftClick)
        {
            if(obj.ObjectKind == ObjectKind.Companion)
            {
                var ownerID = getCompanionOwnerID(obj.Address);
                obj = _objects.SearchById(ownerID);
            }
            this._targetManager.Target = obj;
            
        }
    }

    private void BuildMonitoringList()
    {
        foreach (var obj in _objects)
        {
            if (_players.ContainsKey(obj.ObjectId)) continue;

            if (obj.ObjectKind == ObjectKind.Player ||
                obj.ObjectKind == ObjectKind.Companion)
            {
                _players.Add(obj.ObjectId, obj);
            }
            

        }
    }

    private void CleanMonitoringList()
    {
        foreach (KeyValuePair<uint, GameObject> entry in _players)
        {
            if (_objects.SearchById(entry.Key) is null)
            {
                _players.Remove(entry.Key);
            }
        }
        this.Configuration.CleaningStarted = false;
        this.Configuration.Save();
    }
}
