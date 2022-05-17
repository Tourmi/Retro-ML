using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Game;
using Retro_ML.Neural.Play;
using Retro_ML.Neural.Train;
using Retro_ML.Plugin;
using Retro_ML.Tetris.Configuration;
using Retro_ML.Tetris.Game;
using Retro_ML.Tetris.Neural.Train;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.Tetris
{
    public class TetrisPlugin : IGamePlugin
    {
        public string PluginName => "Tetris";
        public string PluginGameName => "Tetris";
        public string PluginROMHeaderName => "TETRIS";
        public string ConsolePluginName => "GameBoy";
        public string PluginConfigPath => "config/plugins/tetris-config.json";

        public IDataFetcherFactory GetDataFetcherFactory() => new TetrisDataFetcherFactory();

        public INeuralPlayer GetNeuralPlayer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new SharpNeatPlayer(emulatorManager, appConfig);

        public INeuralTrainer GetNeuralTrainer(EmulatorManager emulatorManager, ApplicationConfig appConfig) => new TetrisTrainer(emulatorManager, appConfig);

        public IPluginConfig GetPluginConfig() => new TetrisPluginConfig();
    }
}
