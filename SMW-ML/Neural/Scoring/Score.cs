using SMW_ML.Game.SuperMarioWorld;
using System.Collections.Generic;
using System.Linq;

namespace SMW_ML.Neural.Scoring
{
    internal class Score
    {
        private List<IScoreFactor> scoreFactors;

        public Score()
        {
            scoreFactors = new List<IScoreFactor>()
            {
                new DiedScoreFactor() { ScoreFactor = -10 },
                new DistanceScoreFactor() { ScoreFactor = 3 },
                new StopMovingScoreFactor() { ScoreFactor = -10 },
                new TimeTakenScoreFactor() { ScoreFactor = 1 },
                new WonLevelScoreFactor() { ScoreFactor = 100 }
            };
        }

        public void LevelDone()
        {
            foreach (var scoreFactor in scoreFactors)
            {
                scoreFactor.LevelDone();
            }
        }

        public void Update(DataFetcher dataReader)
        {
            foreach (var scoreFactor in scoreFactors)
            {
                scoreFactor.Update(dataReader);
            }
        }

        public double GetFinalScore() => scoreFactors.Aggregate(0.0, (total, sf) => total + sf.GetFinalScore());

        public bool ShouldStop => scoreFactors.Any(sf => sf.ShouldStop);
    }
}
