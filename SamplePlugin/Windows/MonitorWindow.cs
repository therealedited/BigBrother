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

namespace BigBrother.Windows;

//Many thanks to ascclemens for her PeepingTom plugin.
public class MonitorWindow : Window, IDisposable
{
    private Configuration Configuration;

    private Dictionary<uint, GameObject>? _players = null;

    public MonitorWindow(Plugin plugin) : base(
        "Monitor")
    {
        this.Size = new Vector2(232, 75);
        this.SizeCondition = ImGuiCond.Always;
        this.Configuration = plugin.Configuration;
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

        if (ImGui.BeginListBox("##monitoring", new Vector2(-1, height)))
        {
            AddEntry("Person1", null);
            ImGui.EndListBox();
        }
    }

    private void AddEntry(string name, GameObject? obj, ImGuiSelectableFlags flags = ImGuiSelectableFlags.None)
    {
        ImGui.BeginGroup();

        ImGui.Selectable(name, false, flags);

        ImGui.EndGroup();
    }
}
