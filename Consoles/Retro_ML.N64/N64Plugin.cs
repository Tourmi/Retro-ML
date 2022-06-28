using Retro_ML.Configuration;
using Retro_ML.Game;
using Retro_ML.Plugin;

namespace Retro_ML.N64;
internal class N64Plugin : IConsolePlugin
{
    public string ConsoleName => "Nintendo 64";

    public string[] ROMExtensions => new string[] { "n64", "N64", "u64", "v64", "z64", "d64" };

    public string PluginName => "N64";

    public string PluginConfigPath => "config/plugins/n64-config.json";

    public IInput GetInput() => new N64Input();
    public IPluginConfig GetPluginConfig() => throw new NotImplementedException();
}
