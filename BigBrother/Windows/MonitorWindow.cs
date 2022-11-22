using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Logging;
using Dalamud.Game;
using System.Diagnostics;
using BigBrother.SeFunctions;
using System.Linq;
using BigBrother.Utils;
using Lumina.Excel.GeneratedSheets;

namespace BigBrother.Windows
{

    //Many thanks to ascclemens for her PeepingTom plugin.
    internal partial class MonitorWindow : Window, IDisposable
    {
        protected Dictionary<IntPtr, GameObject>? _players = new Dictionary<IntPtr, GameObject>();
        protected PlaySound _sounds;
        protected Stopwatch counterCleaning = new Stopwatch();
        protected Stopwatch counterUpdate = new Stopwatch();
        protected Vector4 _red = new Vector4(255, 0, 0, 255);
        protected Vector4 _white = new Vector4(255, 255, 255, 255);


        public MonitorWindow() : base(
            "Monitor", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.Size = new Vector2(232, 300);
            this.SizeCondition = ImGuiCond.Once;
            Plugin.Framework.Update += this.OnFrameworkUpdate;
            counterCleaning.Start();
            counterUpdate.Start();
            _sounds = new PlaySound(new SigScanner());
        }

        public void Dispose()
        {
            Plugin.Framework.Update -= this.OnFrameworkUpdate;
        }

        //Thanks https://git.anna.lgbt/ascclemens/PeepingTom/src/branch/main/Peeping%20Tom/TargetWatcher.cs#L48
        public void OnFrameworkUpdate(Framework framework)
        {
            if (counterCleaning.ElapsedMilliseconds > 500)
            {
                if (Plugin.Configuration.TrackPeople)
                {
                    BuildMonitoringList();
                    CleanMonitoringList();
                }
                counterCleaning.Restart();
            }
        }

        public override void Draw()
        {
            if (ImGui.Button("Open Settings", new Vector2(ImGui.GetContentRegionAvail().X, 30)))
            {
                Plugin.WindowSystem.GetWindow("Configuration Window")!.IsOpen = !Plugin.WindowSystem.GetWindow("Configuration Window")!.IsOpen;
            }
            DrawMonitoringListUI();
        }


    }
}
