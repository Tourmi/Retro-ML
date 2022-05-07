using NUnit.Framework;
using Retro_ML.Neural.Train;
using Retro_ML.Neural.Train.StopCondition;
using System;
using System.Threading;

namespace Retro_ML_TEST.Neural.Training
{
    [TestFixture]
    internal class StopConditionTest
    {
        [Test]
        public void Fitness()
        {
            FitnessStopCondition sc = new() { ParamValue = 50 };
            sc.Start();

            TrainingStatistics ts = new();
            ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, 49);
            Assert.IsFalse(sc.CheckShouldStop(ts), "Should not have stopped");

            ts = new();
            ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, 50);
            Assert.IsTrue(sc.CheckShouldStop(ts), "Should have stopped");
        }

        [Test]
        public void GenerationCount()
        {
            GenerationCountStopCondition sc = new() { ParamValue = 50 };
            sc.Start();
            TrainingStatistics ts = new();
            ts.AddStat(TrainingStatistics.CURRENT_GEN, 49);
            Assert.IsFalse(sc.CheckShouldStop(ts), "Should not have stopped");

            ts = new();
            ts.AddStat(TrainingStatistics.CURRENT_GEN, 50);
            Assert.IsTrue(sc.CheckShouldStop(ts), "Should have stopped");
        }

        [Test]
        public void Plateau()
        {
            PlateauStopCondition sc = new() { ParamValue = 50 };
            sc.Start();
            double fitness = 0;
            TrainingStatistics ts;
            for (int i = 0; i < 55; i++)
            {
                fitness += 1;
                ts = new();
                ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, fitness);
                Assert.IsFalse(sc.CheckShouldStop(ts), "Should not have stopped since there is no plateau.");
            }
            for (int i = 0; i < 49; i++)
            {
                ts = new();
                ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, fitness);
                Assert.IsFalse(sc.CheckShouldStop(ts), "Should not have stopped since it hasn't plateaud for long enough.");
            }

            ts = new();
            ts.AddStat(TrainingStatistics.BEST_GENOME_FITNESS, fitness);
            Assert.IsTrue(sc.CheckShouldStop(ts), "Should have stopped.");
        }

        [Test]
        public void Time()
        {
            Assert.Ignore("Testing in real time would take too long");

            TimeStopCondition sc = new() { ParamValue = 2 };
            sc.Start();
            Assert.IsFalse(sc.CheckShouldStop(new TrainingStatistics()), "Should not have stopped yet");
            Thread.Sleep(TimeSpan.FromMinutes(2));
            Assert.IsTrue(sc.CheckShouldStop(new TrainingStatistics()), "Should have stopped after 2 minutes");
        }
    }
}
