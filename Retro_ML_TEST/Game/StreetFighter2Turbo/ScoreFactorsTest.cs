using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.StreetFighter2Turbo.Configuration;
using Retro_ML.StreetFighter2Turbo.Game;
using Retro_ML.StreetFighter2Turbo.Neural.Scoring;
using Retro_ML_TEST.Emulator;

namespace Retro_ML_TEST.Game.StreetFighter2Turbo
{
    [TestFixture]
    internal class ScoreFactorsTest
    {
        private MockEmulatorAdapter? emu;
        private SF2TDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new SF2TDataFetcher(emu, new NeuralConfig(), new SF2TPluginConfig());
        }
    }
}
