using Retro_ML.Configuration;
using Retro_ML.Game;
using Retro_ML.Plugin;

namespace Retro_ML.Famicom
{
    public class FamicomPlugin : IConsolePlugin
    {
        public string PluginName => "NES";
        public string ConsoleName => "NES/Famicom";
        public string[] ROMExtensions => new string[] { "nes" };
        public string PluginConfigPath => "config/plugins/nes-config.json";

        public IInput GetInput() => new FamicomInput();

        public IPluginConfig GetPluginConfig()
        {
            throw new NotImplementedException();
        }
    }
}