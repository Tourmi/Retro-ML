﻿namespace SMW_ML.Utils
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
        public const string SAVESTATES_DIR = "config\\SaveStates\\";
        public const string EMULATOR_ADAPTER = "config/bizhawkAdapter.lua";
        public const string SHARPNEAT_CONFIG = "config/sharpNeatConfig.json";
        public const string APP_CONFIG = "config/appConfig.json";
        public const string ROM = "smw.sfc";

        public const string CURRENT_POPULATION = "current";
        public const string GENOME_DIR = "genomes\\";
        public const string CURRENT_GENOME = "";
    }
}
