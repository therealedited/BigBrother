using System;
using System.Diagnostics.Metrics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Big_Brother.Utils;
using Dalamud.Game;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using ImGuiNET;
using ImGuiScene;

namespace BigBrother.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;
    private Framework _framework;
    private byte[] buffer = new byte[256];
    private static Mutex mutex = new Mutex();
    private int _monitorRange;

    public ConfigWindow(Plugin plugin, Framework framework) : base(
        "Config", ImGuiWindowFlags.None)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.Plugin = plugin;
        this.Configuration = plugin.Configuration;
        _framework = framework;
        _framework.Update += this.OnFrameworkUpdate;
        _monitorRange = Configuration.MonitorRange;
    }

    public void Dispose()
    {
        _framework.Update -= this.OnFrameworkUpdate;
        Configuration.MonitorRange = _monitorRange;
        Configuration.Save();
    }

    public void OnFrameworkUpdate(Framework framework)
    {
        
    }

    public override void Draw()
    {
        var trackPeople = this.Configuration.TrackPeople;
        var trackWeapons = this.Configuration.MonitorWeapons;
        var trackMinions = this.Configuration.MonitorMinions;
        var playSounds = this.Configuration.PlaySounds;

        if (ImGui.BeginTabBar("tabBar", ImGuiTabBarFlags.NoCloseWithMiddleMouseButton))
        {
            if (ImGui.BeginTabItem("Config"))
            {
                if (ImGui.Checkbox("Monitor Area", ref trackPeople))
                {
                    this.Configuration.TrackPeople = trackPeople;
                    this.Configuration.Save();
                }

                ImGui.Separator();

                if (ImGui.Checkbox("Monitor Minions", ref trackMinions))
                {
                    this.Configuration.MonitorMinions = trackMinions;
                    this.Configuration.Save();
                }

                if (ImGui.Checkbox("Monitor Weapons", ref trackWeapons))
                {
                    this.Configuration.MonitorWeapons = trackWeapons;
                    this.Configuration.Save();
                }

                /*if (ImGui.Checkbox("Play Sounds", ref playSounds))
                {
                    this.Configuration.PlaySounds = playSounds;
                    this.Configuration.Save();
                }*/

                if (ImGui.Button("Clean List"))
                {
                    this.Configuration.CleaningStarted = true;
                    this.Configuration.Save();
                }
                if(ImGui.SliderInt("Monitor Radius", ref _monitorRange, 0, 100))
                {
                    Configuration.MonitorRange = _monitorRange;
                }
                
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Ignore List"))
            {
                ImGui.Text("Add player to the ignore list");
                ImGui.InputText("##input", buffer, (uint)buffer.Length);
                ImGui.SameLine();
                if (ImGui.Button("+"))
                {
                    Configuration.ignorePlayers.Add(new Player(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length), ++Configuration.ignoredPlayersNumber));
                    Configuration.Save();
                }
                foreach (Player player in Configuration.ignorePlayers)
                {
                    AddEntry(player);
                }
                ImGui.Separator();
                /* if (ImGui.Button("DEBUG RESET"))
                 {
                     Configuration.ignoredPlayersNumber = 0;
                     Configuration.ignorePlayers.Clear();
                     Configuration.Save();
                 }
                 ImGui.Text($"{Configuration.ignoredPlayersNumber}");
                 foreach (Player p in Configuration.ignorePlayers)
                 {
                     ImGui.Text($"{p.name}");
                 }*/
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Report issues"))
            {
                if (ImGui.Button("Big Brother Discord"))
                {
                    Util.OpenLink("https://discord.gg/qddbpeCWWw");
                }
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        
    }

    public void AddEntry(Player player)
    {
        ImGui.Text(player.name);
        ImGui.SameLine();
        if (ImGui.Button($"DEL##{player.id}"))
        {
            Configuration.ignorePlayers.RemoveAt(player.id-1);
            --Configuration.ignoredPlayersNumber;
            Configuration.Save();

        }
    }
}
