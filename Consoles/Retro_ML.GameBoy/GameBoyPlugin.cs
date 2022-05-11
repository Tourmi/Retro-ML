using Retro_ML.Configuration;
using Retro_ML.Game;
using Retro_ML.Plugin;

namespace Retro_ML.GameBoy
{
    public class GameBoyPlugin : IConsolePlugin
    {
        public string PluginName => "GameBoy";
        public string ConsoleName => "Game Boy Advance";
        public string[] ROMExtensions => new string[] { "gb", "gbc", "gba" };
        public string PluginConfigPath => "config/plugins/gba-config.json";

        public IInput GetInput() => new GameBoyInput();

        public IPluginConfig GetPluginConfig()
        {
            throw new NotImplementedException();
        }
    }
}
