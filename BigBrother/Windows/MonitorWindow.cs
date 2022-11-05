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
using Dalamud.Game;
using System.Diagnostics;
using Big_Brother.SeFunctions;
using System.Collections;
using System.Linq;
using Big_Brother.Utils;
using Lumina.Text;
using System.Text.RegularExpressions;
using Dalamud.Interface;
using System.Drawing;

namespace BigBrother.Windows;

//Many thanks to ascclemens for her PeepingTom plugin.
public class MonitorWindow : Window, IDisposable
{
    private Configuration Configuration;

    private Dictionary<IntPtr, GameObject>? _players = new Dictionary<IntPtr, GameObject>();

    private Plugin _plugin;

    private const int WeaponHidden1 = 0x85F;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private const int WeaponHidden2 = 0x73C; //Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private const byte IsWeaponHidden1 = 0x01;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private const byte IsWeaponHidden2 = 0x02;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
    private ObjectTable _objects;
    private TargetManager _targetManager;
    private Framework _framework;
    private PlaySound _sounds;
    private WindowSystem _windowSystem;
    private Stopwatch counter = new Stopwatch();
    private Vector4 _red = new Vector4(255, 0, 0, 255);
    private Vector4 _white = new Vector4(255, 255, 255, 255);


    public MonitorWindow(Plugin plugin, ObjectTable objects, TargetManager targetManager, Framework framework, WindowSystem windowSystem) : base(
        "Monitor", ImGuiWindowFlags.NoScrollbar)
    {
        this.Size = new Vector2(232, 300);
        this.SizeCondition = ImGuiCond.Once;
        this.Configuration = plugin.Configuration;
        this._plugin = plugin;
        _objects = objects;
        _targetManager = targetManager;
        _framework = framework;
        _framework.Update += this.OnFrameworkUpdate;
        counter.Start();
        _sounds = new PlaySound(new SigScanner());
        _windowSystem = windowSystem;
    }

    public void Dispose()
    {
        _framework.Update -= this.OnFrameworkUpdate;
    }

    //Thanks https://git.anna.lgbt/ascclemens/PeepingTom/src/branch/main/Peeping%20Tom/TargetWatcher.cs#L48
    public void OnFrameworkUpdate(Framework framework)
    {
        if (counter.ElapsedMilliseconds > 5000)
        {
            if (Configuration.TrackPeople)
            {
                PluginLog.Information("Cleaning...");
                CleanMonitoringList();
            }
            counter.Restart();
        }
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
        if (ImGui.Button("Open Settings", new Vector2(width, 30)))
        {
            _windowSystem.GetWindow("Config")!.IsOpen = !_windowSystem.GetWindow("Config")!.IsOpen;
        }
        var listboxopened = ImGui.BeginListBox("###monitoring", new Vector2(width, height - 30));


        if (this.Configuration.CleaningStarted)
        {
            CleanMonitoringList();
        }
        BuildMonitoringList();
        if (listboxopened)
        {
            if (this._plugin.Configuration.TrackPeople)
            {
                foreach (KeyValuePair<IntPtr, GameObject> entry in _players)
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


    //Why?
    public bool IsCharacterIgnored(string name)
    {
        var allIgnoredPlayers = new List<string>();

        foreach (Player p in Configuration.ignorePlayers)
        {
            allIgnoredPlayers.Add(p.name);
        }

        return allIgnoredPlayers.FirstOrDefault(stringToCheck => stringToCheck.Contains(name)) != null;
    }

    public int calculateEuclideanDistance(int x, int z)
    {
        return (int)Math.Sqrt(x*x + z*z);
    }
    private void AddEntry(GameObject? obj, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None) //This function was clearly written with no goal in mind. Do not look unless you like to see spaghetti code.
    {
        if (obj == null) return;

        ImGui.BeginGroup();
        var status = "";

        if (IsCharacterIgnored(obj.Name.TextValue))
        {
            return;
        }

        if (calculateEuclideanDistance(obj.YalmDistanceX, obj.YalmDistanceZ) > Configuration.MonitorRange)
        {
            return;
        }

     

        if (this._plugin.Configuration.MonitorMinions && obj.ObjectKind is ObjectKind.Companion)
        {
            status += $"{obj.YalmDistanceX} - {obj.YalmDistanceZ} | ";
            status += "M";
        }

        if (this._plugin.Configuration.MonitorWeapons && obj.ObjectKind is ObjectKind.Player)
        {
            if (!IsWeaponHidden((Character)obj))
            {
                status += $"{obj.YalmDistanceX} - {obj.YalmDistanceZ} | ";
                status += "W";
            }
        }

        if ((!status.Contains("M") && !(status.Contains("W"))))
        {
            return;
        }


        ImGui.PushStyleColor(ImGuiCol.Text, obj.ObjectKind == ObjectKind.Player ? _white: _red);
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

    private void BuildMonitoringList()
    {
        foreach (var obj in _objects)
        {
            if (_players.ContainsKey(obj.Address)) continue;

            if (obj.ObjectKind == ObjectKind.Player ||
                obj.ObjectKind == ObjectKind.Companion)
            {
                _players.Add(obj.Address, obj);
                if (Configuration.PlaySounds)
                {
                    _sounds.Play(Big_Brother.Utils.Sounds.Sound02);
                }
            }


        }
    }

    private void CleanMonitoringList()
    {
        foreach (KeyValuePair<IntPtr, GameObject> entry in _players)
        {
            if (_objects.SearchById(entry.Value.ObjectId) is null)
            {
                _players.Remove(entry.Key);
            }
        }
        this.Configuration.CleaningStarted = false;
        this.Configuration.Save();
    }
}
