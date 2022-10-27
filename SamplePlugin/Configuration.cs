using Big_Brother.Utils;
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

        public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

        public bool TrackPeople { get; set; } = false;

        public bool MonitorMinions { get; set; } = false;

        public bool MonitorWeapons { get; set; } = false;

        public bool CleaningStarted { get; set; } = false;

        public bool PlaySounds { get; set; } = false;

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
