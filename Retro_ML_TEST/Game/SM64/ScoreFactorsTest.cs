using NUnit.Framework;
using Retro_ML.Configuration;
using Retro_ML.SuperMario64.Configuration;
using Retro_ML.SuperMario64.Game;
using Retro_ML.SuperMario64.Neural.Scoring;
using Retro_ML_TEST.Mocks;
using static Retro_ML.SuperMario64.Game.Addresses;

namespace Retro_ML_TEST.Game.SM64;
[TestFixture]
internal class ScoreFactorsTest
{
    private const double EPSILON = 0.0001f;

    private MockEmulatorAdapter? emu;
    private SM64DataFetcher? df;

    [SetUp]
    public void SetUp()
    {
        emu = new MockEmulatorAdapter();
        df = new SM64DataFetcher(emu, new NeuralConfig(), new SM64PluginConfig());
    }

    [Test]
    public void TimeTakenScoreFactor()
    {
        var sf = new TimeTakenScoreFactor() { ScoreMultiplier = -9, MaximumTrainingTime = 12 }.Clone();
        Assert.IsAssignableFrom<TimeTakenScoreFactor>(sf);

        for (int i = 0; i < 60.0 * 12 - 1; i++)
        {
            df!.NextFrame();
            sf.Update(df);
            Assert.IsFalse(sf.ShouldStop);
        }

        df!.NextFrame();
        sf.Update(df);
        Assert.IsTrue(sf.ShouldStop);
        sf.LevelDone();
        Assert.AreEqual(-9 * 12, sf.GetFinalScore(), 0.00001);

        for (int i = 0; i < 60.0 * 12 - 1; i++)
        {
            df!.NextFrame();
            sf.Update(df);
            Assert.IsFalse(sf.ShouldStop);
        }

        df!.NextFrame();
        sf.Update(df);
        Assert.IsTrue(sf.ShouldStop);
        sf.LevelDone();
        Assert.AreEqual(-9 * 12 * 2, sf.GetFinalScore(), 0.00001);
    }

    [Test]
    public void CoinScoreFactor()
    {
        var sf = new CoinScoreFactor() { ScoreMultiplier = 11 }.Clone();
        Assert.IsAssignableFrom<CoinScoreFactor>(sf);

        emu!.SetMemory(Mario.Coins.Address, 0x10, 0x20);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(0, sf.GetFinalScore(), EPSILON);
        emu!.SetMemory(Mario.Coins.Address, 0x10, 0x25);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(55, sf.GetFinalScore(), EPSILON);

        df!.NextState();
        sf.LevelDone();

        emu!.SetMemory(Mario.Coins.Address, 0x10, 0x27);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(55, sf.GetFinalScore(), EPSILON);
        emu!.SetMemory(Mario.Coins.Address, 0x10, 0x28);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(66, sf.GetFinalScore(), EPSILON);
    }

    [Test]
    public void DiedScoreFactor()
    {
        var sf = new DiedScoreFactor() { ScoreMultiplier = -33 }.Clone();
        Assert.IsAssignableFrom<DiedScoreFactor>(sf);

        emu!.SetMemory(Mario.Health.Address, 8);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(0, sf.GetFinalScore(), EPSILON);

        emu!.SetMemory(Mario.Health.Address, 0);
        df.NextFrame();
        sf.Update(df);
        Assert.IsTrue(sf.ShouldStop);
        Assert.AreEqual(-33, sf.GetFinalScore(), EPSILON);

        sf.LevelDone();
        emu!.SetMemory(Mario.Health.Address, 8);
        df.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(-33, sf.GetFinalScore(), EPSILON);

        emu!.SetMemory(Mario.Action.Address, 0x00, 0x02, 0x13, 0x15);
        df.NextFrame();
        sf.Update(df);
        Assert.IsTrue(sf.ShouldStop);
        Assert.AreEqual(-66, sf.GetFinalScore(), EPSILON);
    }

    [Test]
    public void StarScoreFactor()
    {
        var sf = new StarScoreFactor() { ScoreMultiplier = 111 }.Clone();
        Assert.IsAssignableFrom<StarScoreFactor>(sf);

        emu!.SetMemory(Progress.StarCount.Address, 0x10, 0x20);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(0, sf.GetFinalScore(), EPSILON);
        emu!.SetMemory(Progress.StarCount.Address, 0x10, 0x25);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsTrue(sf.ShouldStop);
        Assert.AreEqual(555, sf.GetFinalScore(), EPSILON);

        df!.NextState();
        sf.LevelDone();

        emu!.SetMemory(Progress.StarCount.Address, 0x10, 0x27);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsFalse(sf.ShouldStop);
        Assert.AreEqual(555, sf.GetFinalScore(), EPSILON);
        emu!.SetMemory(Progress.StarCount.Address, 0x10, 0x28);
        df!.NextFrame();
        sf.Update(df);
        Assert.IsTrue(sf.ShouldStop);
        Assert.AreEqual(666, sf.GetFinalScore(), EPSILON);
    }

    [Test]
    public void ExplorationScoreFactor()
    {
        var sf = new ExplorationScoreFactor() { ScoreMultiplier = 1 }.Clone();
        Assert.IsAssignableFrom<ExplorationScoreFactor>(sf);
    }

    [Test]
    public void DistanceToStarScoreFactor()
    {
        var sf = new DistanceToStarScoreFactor() { ScoreMultiplier = 1 }.Clone();
        Assert.IsAssignableFrom<DistanceToStarScoreFactor>(sf);
    }
}
