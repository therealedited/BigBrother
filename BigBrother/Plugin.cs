using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Reflection;
using Dalamud.Interface.Windowing;
using BigBrother.Windows;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState;
using Dalamud.Game;

namespace BigBrother
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Big Brother";
        private const string ConfigCommand = "/bb config";
        private const string MonitorCommand = "/bb";
        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("BigBrother");

        public MonitorWindow monitorWindow;
        [PluginService][RequiredVersion("1.0")] public static ObjectTable Objects { get; private set; } = null!;

        [PluginService][RequiredVersion("1.0")] public static TargetManager TargetManager{ get; private set; } = null!;

        [PluginService][RequiredVersion("1.0")] public static Framework Framework { get; private set; } = null!;



        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            // you might normally want to embed resources and load them from the manifest stream
            //var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");
            //var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);
            monitorWindow = new MonitorWindow(this, Objects, TargetManager, Framework, WindowSystem);
            WindowSystem.AddWindow(new ConfigWindow(this, Framework));
            WindowSystem.AddWindow(monitorWindow);

            this.CommandManager.AddHandler(ConfigCommand, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the config window."
            });

            this.CommandManager.AddHandler(MonitorCommand, new CommandInfo(OnCommand)
            {
                HelpMessage = "Opens the monitor window."
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            
        }

        public void Dispose()
        {
            monitorWindow.Dispose();
            this.WindowSystem.RemoveAllWindows();
            this.CommandManager.RemoveHandler(ConfigCommand);
            this.CommandManager.RemoveHandler(MonitorCommand);
        }

        private void OnCommand(string command, string args)
        {
            switch (args) {
               case "":
                    WindowSystem.GetWindow("Monitor")!.IsOpen = true;
                    break;
                case "config":
                    WindowSystem.GetWindow("Config")!.IsOpen = true;
                    break;
            }
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            WindowSystem.GetWindow("Config")!.IsOpen = true;
        }
    }
}
