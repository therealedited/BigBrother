using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Big_Brother.Utils;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using ImGuiNET;
using ImGuiScene;

namespace BigBrother.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;
    private byte[] buffer = new byte[256];
    private static Mutex mutex = new Mutex();

    public ConfigWindow(Plugin plugin) : base(
        "Config", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        this.Plugin = plugin;
        this.Configuration = plugin.Configuration;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGuiTabBarFlags tabBarFlags = ImGuiTabBarFlags.None;

        var trackPeople = this.Configuration.TrackPeople;
        var trackWeapons = this.Configuration.MonitorWeapons;
        var trackMinions = this.Configuration.MonitorMinions;
        var playSounds = this.Configuration.PlaySounds;

        if (ImGui.BeginTabBar("tabBar", tabBarFlags))
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
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Debug"))
            {
                ImGui.Text($"TrackPeople: {this.Plugin.Configuration.TrackPeople}");
                ImGui.Text($"MonitorMinions: {this.Plugin.Configuration.MonitorMinions}");
                ImGui.Text($"MonitorWeapons: {this.Plugin.Configuration.MonitorWeapons}");
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
                if (ImGui.Button("DEBUG RESET"))
                {
                    Configuration.ignoredPlayersNumber = 0;
                    Configuration.ignorePlayers.Clear();
                    Configuration.Save();
                }
                ImGui.Text($"{Configuration.ignoredPlayersNumber}");
                foreach (Player p in Configuration.ignorePlayers)
                {
                    ImGui.Text($"{p.name}");
                }
                
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
            mutex.WaitOne();
            Configuration.ignorePlayers.RemoveAt(player.id-1);
            --Configuration.ignoredPlayersNumber;
            Configuration.Save();
            mutex.ReleaseMutex();
        }
    }
}
