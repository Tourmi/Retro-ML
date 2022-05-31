namespace Retro_ML.Utils
{
    /// <summary>
    /// This static class is used to store the information to any filesystem related paths.
    /// </summary>
    internal static class DefaultPaths
    {
        public const string POPULATION_EXTENSION = ".pop";
        public const string GENOME_EXTENSION = ".genome";

        public const string EMULATOR = "emu/EmuHawk.exe";
        public const string EMULATOR_CONFIG = "config/bizhawkConfig.ini";
        public const string EMULATOR_PLAY_CONFIG = "config/bizhawkConfigPlayMode.ini";
        public const string SAVESTATES_DIR = "config\\SaveStates\\";
        public const string EMULATOR_ADAPTER = "config/bizhawkAdapter.lua";
        public const string SHARPNEAT_CONFIG = "config/sharpNeatConfig.json";
        public const string APP_CONFIG = "config/appConfig.json";
        public const string NEURAL_CONFIG_EXTENSION = "ncfg";
        public const string NEURAL_CONFIG_NAME = $"neuralConfig.{NEURAL_CONFIG_EXTENSION}";
        public const string GAME_PLUGIN_CONFIG_EXTENSION = "pcfg";
        public const string GAME_PLUGIN_CONFIG_NAME = $"gamePluginConfig.{GAME_PLUGIN_CONFIG_EXTENSION}";

        public const string PLUGINS_DIR = "plugins/";

        public const string CURRENT_POPULATION = "current";
        public const string GENOME_DIR = "genomes\\";
        public const string CURRENT_GENOME = "";
    }
}
