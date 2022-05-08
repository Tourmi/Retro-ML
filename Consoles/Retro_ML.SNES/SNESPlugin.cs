using Retro_ML.Configuration;
using Retro_ML.Plugin;

namespace Retro_ML.SNES
{
    public class SNESPlugin : IConsolePlugin
    {
        public string PluginName => "SNES";
        public string ConsoleName => "Super Nintendo/Super Famicom";
        public string[] ROMExtensions => new string[] { "sfc", "smc" };
        public string PluginConfigPath => "config/plugins/snes-config.json";

        public IGamePluginConfig GetPluginConfig()
        {
            throw new NotImplementedException();
        }
    }
}