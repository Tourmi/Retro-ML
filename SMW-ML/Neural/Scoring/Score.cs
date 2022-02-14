using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using System.Collections.Generic;
using System.Linq;

namespace SMW_ML.Neural.Scoring
{
    internal class Score
    {
        private List<IScoreFactor> scoreFactors;

        public Score(ApplicationConfig config)
        {
            scoreFactors = config.GetScoreFactorClones().ToList();
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
