using NUnit.Framework;
using Retro_ML.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Retro_ML.PokemonGen1.Configuration;
using Retro_ML.PokemonGen1.Game;
using Retro_ML.PokemonGen1.Neural.Scoring;
using Retro_ML_TEST.Mocks;

namespace Retro_ML_TEST.Game.PokemonGen1
{
    [TestFixture]
    internal class ScoreFactorTest
    {
        private MockEmulatorAdapter? emu;
        private PokemonDataFetcher? df;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            df = new PokemonDataFetcher(emu, new NeuralConfig(), new PokemonPluginConfig());
        }

        [Test]
        public void WonFightScoreFactor()
        {
            var sf = new WonFightScoreFactor() { ScoreMultiplier = 10 }.Clone();
            Assert.IsAssignableFrom<WonFightScoreFactor>(sf);
            emu!.SetMemory(Addresses.OpposingPokemon.CurrentHP.Address, 50);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.OpposingPokemon.CurrentHP.Address, 0);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(10, sf.GetFinalScore());
        }

        [Test]
        public void LostFightScoreFactor()
        {
            var sf = new LostFightScoreFactor() { ScoreMultiplier = -8 }.Clone();
            Assert.IsAssignableFrom<LostFightScoreFactor>(sf);
            emu!.SetMemory(Addresses.CurrentPokemon.CurrentHP.Address, 50);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.CurrentPokemon.CurrentHP.Address, 0);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(-8, sf.GetFinalScore());
        }

        [Test]
        public void FightCanceledScoreFactor()
        {
            var sf = new FightCanceledScoreFactor() { ScoreMultiplier = -2 }.Clone();
            Assert.IsAssignableFrom<FightCanceledScoreFactor>(sf);
            emu!.SetMemory(Addresses.GameState.Address, 1);
            sf.Update(df!);
            Assert.AreEqual(0, sf.GetFinalScore());
            emu!.SetMemory(Addresses.GameState.Address, 0);
            df!.NextFrame();
            sf.Update(df!);
            Assert.AreEqual(-2, sf.GetFinalScore());
        }
    }
}

