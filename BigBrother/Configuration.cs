using BigBrother.Utils;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace BigBrother
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool TrackPeople { get; set; } = false;

        public bool MonitorMinions { get; set; } = false;

        public bool MonitorWeapons { get; set; } = false;

        public bool PlaySounds { get; set; } = false;

        public int MonitorRange { get; set; } = 30;

        public Sounds SoundPlayer { get; set; } = Sounds.Sound01;
        public string SoundPlayer_s { get; set; } = "Sound01";

        public Sounds SoundMinion { get; set; } = Sounds.Sound02;
        public string SoundMinion_s { get; set; } = "Sound02";

        public List<Player> ignorePlayers = new List<Player>();

        public int ignoredPlayersNumber = 0;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
