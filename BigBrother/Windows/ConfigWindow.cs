using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using BigBrother.SeFunctions;
using BigBrother.Utils;
using Dalamud.Game;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BigBrother.Windows
{

    internal partial class ConfigWindow : Window, IDisposable
    {
        private int _monitorRange;
        private PlaySound _sounds;


        public ConfigWindow() : base(
            "Configuration Window", ImGuiWindowFlags.None)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            _monitorRange = Plugin.Configuration.MonitorRange;
            _sounds = new PlaySound(new SigScanner());
        }

        public void Dispose()
        {
            Plugin.Configuration.MonitorRange = _monitorRange;
            Plugin.Configuration.Save();
        }

        public override void Draw()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabBar("##tabBar"), ImGui.EndTabBar))
                return;

            DrawConfigUI();
            DrawIgnorePlayerUI();
            DrawReportIssuesUI();
        }
    }
}
