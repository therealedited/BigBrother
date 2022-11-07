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
        protected Plugin _plugin;
        private Framework _framework;
        private int _monitorRange;
        private PlaySound _sounds;


        public ConfigWindow(Plugin plugin, Framework framework) : base(
            "Configuration Window", ImGuiWindowFlags.None)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(375, 330),
                MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
            };
            _plugin = plugin;
            _framework = framework;
            _framework.Update += this.OnFrameworkUpdate;
            _monitorRange = _plugin.Configuration.MonitorRange;
            _sounds = new PlaySound(new SigScanner());
        }

        public void Dispose()
        {
            _framework.Update -= this.OnFrameworkUpdate;
            _plugin.Configuration.MonitorRange = _monitorRange;
            _plugin.Configuration.Save();
        }

        public void OnFrameworkUpdate(Framework framework)
        {

        }

        public override void Draw()
        {
            if (_plugin is null) return;

            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabBar("##tabBar"), ImGui.EndTabBar))
                return;

            DrawConfigUI();
            DrawIgnorePlayerUI();
            DrawReportIssuesUI();
        }
    }
}
