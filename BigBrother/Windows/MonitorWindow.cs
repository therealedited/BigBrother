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
using Big_Brother.SeFunctions;
using System.Linq;
using BigBrother.Utils;

namespace BigBrother.Windows
{

    //Many thanks to ascclemens for her PeepingTom plugin.
    internal partial class MonitorWindow : Window, IDisposable
    {
        private Configuration _configuration;

        protected Dictionary<IntPtr, GameObject>? _players = new Dictionary<IntPtr, GameObject>();
        protected Dictionary<IntPtr, GameObject>? _trackedPlayers = new Dictionary<IntPtr, GameObject>();

        protected Plugin _plugin;

        protected const int WeaponHidden1 = 0x85F;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
        protected const int WeaponHidden2 = 0x73C; //Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
        protected const byte IsWeaponHidden1 = 0x01;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
        protected const byte IsWeaponHidden2 = 0x02;//Thanks https://github.com/Ottermandias/Glamourer/blob/main/Glamourer/Offsets.cs
        protected ObjectTable _objects;
        protected TargetManager _targetManager;
        protected Framework _framework;
        protected PlaySound _sounds;
        protected WindowSystem _windowSystem;
        protected Stopwatch counterCleaning = new Stopwatch();
        protected Stopwatch counterUpdate = new Stopwatch();
        protected Vector4 _red = new Vector4(255, 0, 0, 255);
        protected Vector4 _white = new Vector4(255, 255, 255, 255);


        public MonitorWindow(Plugin plugin, ObjectTable objects, TargetManager targetManager, Framework framework, WindowSystem windowSystem) : base(
            "Monitor", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            this.Size = new Vector2(232, 300);
            this.SizeCondition = ImGuiCond.Once;
            this._configuration = plugin.Configuration;
            this._plugin = plugin;
            _objects = objects;
            _targetManager = targetManager;
            _framework = framework;
            _framework.Update += this.OnFrameworkUpdate;
            counterCleaning.Start();
            counterUpdate.Start();
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
            if (counterCleaning.ElapsedMilliseconds > 500)
            {
                if (_configuration.TrackPeople)
                {
                    _trackedPlayers = BuildMonitoringList();
                }
                counterCleaning.Restart();
            }
        }

        public override void Draw()
        {
            if (ImGui.Button("Open Settings", new Vector2(ImGui.GetContentRegionAvail().X, 30)))
            {
                _windowSystem.GetWindow("Configuration Window")!.IsOpen = !_windowSystem.GetWindow("Configuration Window")!.IsOpen;
            }
            DrawMonitoringListUI();
        }


    }
}
