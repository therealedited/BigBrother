using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Configuration;
using ImGuiNET;
using ImGuiScene;

namespace BigBrother.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Plugin Plugin;
    private Configuration Configuration;

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

                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Debug"))
            {
                ImGui.Text($"TrackPeople: {this.Plugin.Configuration.TrackPeople}");
                ImGui.Text($"MonitorMinions: {this.Plugin.Configuration.MonitorMinions}");
                ImGui.Text($"MonitorWeapons: {this.Plugin.Configuration.MonitorWeapons}");
            }
            ImGui.EndTabBar();
        }
    }
}
