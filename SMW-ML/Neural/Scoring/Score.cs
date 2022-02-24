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

        public double GetFinalScore()
        {
            var score = scoreFactors.Aggregate(0.0, (total, sf) => total + sf.GetFinalScore());

            //If the score is negative, force it to a value between 0 and 1
            //Else, add 1, so a positive score remains bigger than a negative one
            //We need to do this since SharpNeat has no support for negative fitness
            if (score < 0) score = -1 / (score - 1);
            else score += 1;

            return score;
        }


        public bool ShouldStop => scoreFactors.Any(sf => sf.ShouldStop);
    }
}
