using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.PokemonGen1.Configuration;
using Retro_ML.PokemonGen1.Game;
using Retro_ML_TEST.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML_TEST.Game.PokemonGen1
{
    [TestFixture]
    internal class DataFetcherTest
    {
        private const double EPSILON = 0.00001;

        private PokemonDataFetcher? df;
        private MockEmulatorAdapter? emu;

        [SetUp]
        public void SetUp()
        {
            emu = new MockEmulatorAdapter();
            emu.SetMemory(Addresses.PlayerPokemons.EndOfList.Address, 0);
            df = new PokemonDataFetcher(emu, new NeuralConfig(), new PokemonPluginConfig());
            df.NextState();
        }

        [Test]
        public void NextFrame()
        {
            Assert.DoesNotThrow(() => df!.NextFrame());
        }

        [Test]
        public void NextLevel()
        {
            Assert.DoesNotThrow(() => df!.NextState());
        }

        [Test]
        public void GetOpposingPokemonSleep()
        {
            Assert.AreEqual(0, df!.GetOpposingPokemonSleep());

            emu!.SetMemory(Addresses.OpposingPokemon.StatusEffect.Address, 0b0000_0111);

            df.NextFrame();
            Assert.AreEqual(1, df!.GetOpposingPokemonSleep());
        }

        [Test]
        public void GetOpposingPokemonParalysis()
        {
            Assert.AreEqual(false, df!.GetOpposingPokemonParalysis());

            emu!.SetMemory(Addresses.OpposingPokemon.StatusEffect.Address, 0b0100_0000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetOpposingPokemonParalysis());
        }

        [Test]
        public void GetOpposingPokemonFrozen()
        {
            Assert.AreEqual(false, df!.GetOpposingPokemonFrozen());

            emu!.SetMemory(Addresses.OpposingPokemon.StatusEffect.Address, 0b0010_0000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetOpposingPokemonFrozen());
        }

        [Test]
        public void GetOpposingPokemonBurned()
        {
            Assert.AreEqual(false, df!.GetOpposingPokemonBurned());

            emu!.SetMemory(Addresses.OpposingPokemon.StatusEffect.Address, 0b0001_0000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetOpposingPokemonBurned());
        }

        [Test]
        public void GetOpposingPokemonPoisoned()
        {
            Assert.AreEqual(false, df!.GetOpposingPokemonPoisoned());

            emu!.SetMemory(Addresses.OpposingPokemon.StatusEffect.Address, 0b0000_1000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetOpposingPokemonPoisoned());
        }

        [Test]
        public void GetSleep()
        {
            Assert.AreEqual(0, df!.GetSleep());

            emu!.SetMemory(Addresses.CurrentPokemon.StatusEffect.Address, 0b0000_0111);

            df.NextFrame();
            Assert.AreEqual(1, df!.GetSleep());
        }

        [Test]
        public void GetParalysis()
        {
            Assert.AreEqual(false, df!.GetParalysis());

            emu!.SetMemory(Addresses.CurrentPokemon.StatusEffect.Address, 0b0100_0000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetParalysis());
        }

        [Test]
        public void GetFrozen()
        {
            Assert.AreEqual(false, df!.GetFrozen());

            emu!.SetMemory(Addresses.CurrentPokemon.StatusEffect.Address, 0b0010_0000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetFrozen());
        }

        [Test]
        public void GetBurned()
        {
            Assert.AreEqual(false, df!.GetBurned());

            emu!.SetMemory(Addresses.CurrentPokemon.StatusEffect.Address, 0b0001_0000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetBurned());
        }

        [Test]
        public void GetPoisoned()
        {
            Assert.AreEqual(false, df!.GetPoisoned());

            emu!.SetMemory(Addresses.CurrentPokemon.StatusEffect.Address, 0b0000_1000);

            df.NextFrame();
            Assert.AreEqual(true, df!.GetPoisoned());
        }

        [Test]
        public void IsPokemonYellow()
        {
            Assert.AreEqual(false, df!.IsPokemonYellow);

            emu!.SetMemory(Addresses.PlayerPokemons.EndOfList.Address, 1);
            df.NextState();
            Assert.AreEqual(true, df!.IsPokemonYellow);
        }

        [Test]
        public void IsSuperEffective()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMoveType.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type1.Address, 7);
            emu!.SetMemory(Addresses.OpposingPokemon.Type2.Address, 2);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsSuperEffective());

            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMoveType.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type1.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type2.Address, 2);
            df!.NextFrame();
            Assert.AreEqual(false, df!.IsSuperEffective());
        }

        [Test]
        public void IsNotVeryEffective()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMoveType.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type1.Address, 7);
            emu!.SetMemory(Addresses.OpposingPokemon.Type2.Address, 2);
            df!.NextFrame();
            Assert.AreEqual(false, df!.IsNotVeryEffective());

            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMoveType.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type1.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type2.Address, 2);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsNotVeryEffective());

            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMoveType.Address, 4);
            emu!.SetMemory(Addresses.OpposingPokemon.Type1.Address, 20);
            emu!.SetMemory(Addresses.OpposingPokemon.Type2.Address, 2);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsNotVeryEffective());
        }

        [Test]
        public void IsStab()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMoveType.Address, 8);
            df!.NextFrame();
            Assert.AreEqual(false, df!.IsSTAB());

            emu!.SetMemory(Addresses.CurrentPokemon.Type1.Address, 8);
            emu!.SetMemory(Addresses.CurrentPokemon.Type2.Address, 3);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsSTAB());

            emu!.SetMemory(Addresses.CurrentPokemon.Type1.Address, 26);
            emu!.SetMemory(Addresses.CurrentPokemon.Type2.Address, 8);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsSTAB());
        }

        [Test]
        public void CurrentHP()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.CurrentHP.Address, 0, 100);
            emu!.SetMemory(Addresses.CurrentPokemon.MaxHP.Address, 0, 100);
            df!.NextFrame();
            Assert.AreEqual(1, df!.CurrentHP());

            emu!.SetMemory(Addresses.CurrentPokemon.CurrentHP.Address, 0, 50);
            emu!.SetMemory(Addresses.CurrentPokemon.MaxHP.Address, 0, 100);
            df!.NextFrame();
            Assert.AreEqual(0.5, df!.CurrentHP());
        }

        [Test]
        public void OpposingCurrentHP()
        {
            emu!.SetMemory(Addresses.OpposingPokemon.CurrentHP.Address, 0, 100);
            emu!.SetMemory(Addresses.OpposingPokemon.MaxHP.Address, 0, 100);
            df!.NextFrame();
            Assert.AreEqual(1, df!.OpposingCurrentHP());

            emu!.SetMemory(Addresses.OpposingPokemon.CurrentHP.Address, 0, 50);
            emu!.SetMemory(Addresses.OpposingPokemon.MaxHP.Address, 0, 100);
            df!.NextFrame();
            Assert.AreEqual(0.5, df!.OpposingCurrentHP());
        }

        [Test]
        public void GetAttack() => TestStat(Addresses.CurrentPokemon.Attack, df!.GetAttack, PokemonDataFetcher.MAXIMUM_ATTACK_STAT);

        [Test]
        public void GetDefense() => TestStat(Addresses.CurrentPokemon.Defense, df!.GetDefense, PokemonDataFetcher.MAXIMUM_DEFENSE_STAT);

        [Test]
        public void GetSpeed() => TestStat(Addresses.CurrentPokemon.Speed, df!.GetSpeed, PokemonDataFetcher.MAXIMUM_SPEED_STAT);

        [Test]
        public void GetSpecial() => TestStat(Addresses.CurrentPokemon.Special, df!.GetSpecial, PokemonDataFetcher.MAXIMUM_SPECIAL_STAT);

        [Test]
        public void GetOpposingAttack() => TestStat(Addresses.OpposingPokemon.Attack, df!.GetOpposingAttack, PokemonDataFetcher.MAXIMUM_ATTACK_STAT);

        [Test]
        public void GetOpposingDefense() => TestStat(Addresses.OpposingPokemon.Defense, df!.GetOpposingDefense, PokemonDataFetcher.MAXIMUM_DEFENSE_STAT);

        [Test]
        public void GetOpposingSpeed() => TestStat(Addresses.OpposingPokemon.Speed, df!.GetOpposingSpeed, PokemonDataFetcher.MAXIMUM_SPEED_STAT);

        [Test]
        public void GetOpposingSpecial() => TestStat(Addresses.OpposingPokemon.Special, df!.GetOpposingSpecial, PokemonDataFetcher.MAXIMUM_SPECIAL_STAT);

        [Test]
        public void GetAttackModifier() => TestModifier(Addresses.CurrentPokemon.AttackModifier, df!.GetAttackModifier);

        [Test]
        public void GetDefenseModifier() => TestModifier(Addresses.CurrentPokemon.DefenseModifier, df!.GetDefenseModifier);

        [Test]
        public void GetSpeedModifier() => TestModifier(Addresses.CurrentPokemon.SpeedModifier, df!.GetSpeedModifier);

        [Test]
        public void GetSpecialModifier() => TestModifier(Addresses.CurrentPokemon.SpecialModifier, df!.GetSpecialModifier);

        [Test]
        public void GetAccuracyModifier() => TestModifier(Addresses.CurrentPokemon.AccuracyModifier, df!.GetAccuracyModifier);

        [Test]
        public void GetEvasionModifier() => TestModifier(Addresses.CurrentPokemon.EvasionModifier, df!.GetEvasionModifier);

        [Test]
        public void GetOpposingAttackModifier() => TestModifier(Addresses.OpposingPokemon.AttackModifier, df!.GetOpposingAttackModifier);

        [Test]
        public void GetOpposingDefenseModifier() => TestModifier(Addresses.OpposingPokemon.DefenseModifier, df!.GetOpposingDefenseModifier);

        [Test]
        public void GetOpposingSpeedModifier() => TestModifier(Addresses.OpposingPokemon.SpeedModifier, df!.GetOpposingSpeedModifier);

        [Test]
        public void GetOpposingSpecialModifier() => TestModifier(Addresses.OpposingPokemon.SpecialModifier, df!.GetOpposingSpecialModifier);

        [Test]
        public void GetOpposingAccuracyModifier() => TestModifier(Addresses.OpposingPokemon.AccuracyModifier, df!.GetOpposingAccuracyModifier);

        [Test]
        public void GetOpposingEvasionModifier() => TestModifier(Addresses.OpposingPokemon.EvasionModifier, df!.GetOpposingEvasionModifier);

        [Test]
        public void WonFight()
        {
            emu!.SetMemory(Addresses.OpposingPokemon.CurrentHP.Address, 50);
            df!.NextFrame();
            Assert.AreEqual(false, df!.WonFight());

            emu!.SetMemory(Addresses.OpposingPokemon.CurrentHP.Address, 0);
            df!.NextFrame();
            Assert.AreEqual(true, df!.WonFight());
        }

        [Test]
        public void LostFight()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.CurrentHP.Address, 50);
            df!.NextFrame();
            Assert.AreEqual(false, df!.LostFight());

            emu!.SetMemory(Addresses.CurrentPokemon.CurrentHP.Address, 0);
            df!.NextFrame();
            Assert.AreEqual(true, df!.LostFight());
        }

        [Test]
        public void InFight()
        {
            emu!.SetMemory(Addresses.GameState.Address, 0);
            df!.NextFrame();
            Assert.AreEqual(false, df!.InFight());

            emu!.SetMemory(Addresses.GameState.Address, 1);
            df!.NextFrame();
            Assert.AreEqual(true, df!.InFight());

            emu!.SetMemory(Addresses.GameState.Address, 2);
            df!.NextFrame();
            Assert.AreEqual(true, df!.InFight());
        }

        [Test]
        public void SelectedMovePower()
        {
            Assert.AreEqual(0, df!.SelectedMovePower());

            emu!.SetMemory(Addresses.CurrentPokemon.SelectedMovePower.Address, 50);
            df!.NextFrame();
            Assert.AreEqual(50 / (double)PokemonDataFetcher.MAXIMUM_MOVE_POWER, df!.SelectedMovePower());
        }

        [Test]
        public void Move1Exists() => TestMoveExists(Addresses.CurrentPokemon.MoveIDs, df!.Move1Exists, 0);

        [Test]
        public void Move2Exists() => TestMoveExists(Addresses.CurrentPokemon.MoveIDs, df!.Move2Exists, 1);

        [Test]
        public void Move3Exists() => TestMoveExists(Addresses.CurrentPokemon.MoveIDs, df!.Move3Exists, 2);

        [Test]
        public void Move4Exists() => TestMoveExists(Addresses.CurrentPokemon.MoveIDs, df!.Move4Exists, 3);

        [Test]
        public void IsFightOptionSelected()
        {
            emu!.SetMemory(Addresses.FightCursor.Address, 193);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsFightOptionSelected());
        }

        [Test]
        public void IsOnAwakenDialog()
        {
            emu!.SetMemory(Addresses.WokeUpDialog.Address, 192);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsOnAwakenDialog());
        }

        [Test]
        public void GetMovePP()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.MovesPP.Address, 5, 10, 17, 27);
            df!.NextFrame();
            Assert.AreEqual(5, df!.GetMovePP(0));
            Assert.AreEqual(10, df!.GetMovePP(1));
            Assert.AreEqual(17, df!.GetMovePP(2));
            Assert.AreEqual(27, df!.GetMovePP(3));
        }

        [Test]
        public void IsMoveDisabled()
        {
            emu!.SetMemory(Addresses.CurrentPokemon.MoveIDs.Address, 4, 5, 7, 84);
            emu!.SetMemory(Addresses.CurrentPokemon.DisabledMove.Address, 4);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsMoveDisabled(0));
            Assert.AreEqual(false, df!.IsMoveDisabled(1));
            Assert.AreEqual(false, df!.IsMoveDisabled(2));
            Assert.AreEqual(false, df!.IsMoveDisabled(3));

            emu!.SetMemory(Addresses.CurrentPokemon.DisabledMove.Address, 0);
            df!.NextFrame();
            Assert.AreEqual(false, df!.IsMoveDisabled(0));
        }

        [Test]
        public void IsPlayerTrapped()
        {
            emu!.SetMemory(Addresses.OpposingPokemon.BattleStatus.Address, 0b0010_0000);
            df!.NextFrame();
            Assert.AreEqual(true, df!.IsPlayerTrapped());
            
            emu!.SetMemory(Addresses.OpposingPokemon.BattleStatus.Address, 0b0000_0000);
            df!.NextFrame();
            Assert.AreEqual(false, df!.IsPlayerTrapped());
        }

        [Test]
        public void GetMoveCursorIndex()
        {
            Assert.AreEqual(0, df!.GetMoveCursorIndex());

            emu!.SetMemory(Addresses.MoveCursorIndex.Address, 1);
            df!.NextFrame();
            Assert.AreEqual(1, df!.GetMoveCursorIndex());
        }

        private void TestStat(Addresses.AddressData addr, Func<double> function, int maxValue)
        {
            Assert.AreEqual(0, function(), EPSILON);

            emu!.SetMemory(addr.Address, 0, 10);
            df!.NextFrame();
            Assert.AreEqual(10 / (double)maxValue, function(), EPSILON);
        }

        private void TestModifier(Addresses.AddressData addr, Func<double> function)
        {
            emu!.SetMemory(addr.Address, 7);
            df!.NextFrame();
            Assert.AreEqual(0, function(), EPSILON);

            emu!.SetMemory(addr.Address, 2);
            df!.NextFrame();
            Assert.AreEqual((2 - 7) / 6.0, function(), EPSILON);

            emu!.SetMemory(addr.Address, 10);
            df!.NextFrame();
            Assert.AreEqual((10 - 7) / 6.0, function(), EPSILON);
        }

        private void TestMoveExists(Addresses.AddressData addr, Func<bool> function, uint moveID)
        {
            Assert.AreEqual(false, function());

            emu!.SetMemory(Addresses.CurrentPokemon.MoveIDs.Address + moveID, 0);
            df!.NextFrame();
            Assert.AreEqual(false, function());

            emu!.SetMemory(Addresses.CurrentPokemon.MoveIDs.Address + moveID, 50);
            df!.NextFrame();
            Assert.AreEqual(true, function());
        }

    }
}
