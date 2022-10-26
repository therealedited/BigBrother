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


    public MonitorWindow(Plugin plugin, ObjectTable objects) : base(
        "Monitor")
    {
        this.Size = new Vector2(232, 300);
        this.SizeCondition = ImGuiCond.Always;
        this.Configuration = plugin.Configuration;
        this._plugin = plugin;
        _objects = objects;
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

        BuildMonitoringList();
        if (ImGui.BeginListBox("monitoring", new Vector2(-1, height)))
        {
            foreach (KeyValuePair<uint, GameObject> entry in _players)
            {
                AddEntry(entry.Value);
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

    private void AddEntry(GameObject? obj, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.BeginGroup();
        var status = "";

        if (obj.ObjectKind is ObjectKind.Companion)
        {
            status += "M";
        }

        if (obj is Character && obj.ObjectKind == ObjectKind.Player)
        {
            if (!IsWeaponHidden((Character)obj))
            {
                status += "W";
            }
        }

        if ((status is not "M") && (status is not "W"))
        {
            return;
        }

        ImGui.Selectable(obj.Name.TextValue, false, flags);

        var windowWidth = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;
        ImGui.SameLine(windowWidth - ImGui.CalcTextSize(status).X);

        ImGui.TextUnformatted(status);

        ImGui.EndGroup();
    }

    private void BuildMonitoringList()
    {
        foreach (var obj in _objects)
        {
            if (_players.ContainsKey(obj.ObjectId)) continue;

            if (obj.ObjectKind == ObjectKind.Player ||
                obj.ObjectKind == ObjectKind.Companion)
            {
                PluginLog.Information("Adding: ", obj.Name.TextValue);
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
    }
}
