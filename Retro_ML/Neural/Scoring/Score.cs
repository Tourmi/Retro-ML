using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Models.Config;
using System.Collections.Generic;
using System.Linq;

namespace SMW_ML.Neural.Scoring
{
    /// <summary>
    /// Class that manages the score of a single training session.
    /// </summary>
    internal class Score
    {
        private List<IScoreFactor> scoreFactors;

        public Score(ApplicationConfig config) : this(config.GetScoreFactorClones()) { }

        public Score(IEnumerable<IScoreFactor> scoreFactors)
        {
            this.scoreFactors = scoreFactors.ToList();
        }

        /// <summary>
        /// Should be called whenever a level is done, be it by dying, timing out, or clearing it.
        /// </summary>
        public void LevelDone()
        {
            foreach (var scoreFactor in scoreFactors)
            {
                scoreFactor.LevelDone();
            }
        }

        /// <summary>
        /// Should be called on each frame.
        /// </summary>
        /// <param name="dataReader"></param>
        public void Update(DataFetcher dataReader)
        {
            foreach (var scoreFactor in scoreFactors)
            {
                scoreFactor.Update(dataReader);
            }
        }

        /// <summary>
        /// Returns the final score of the training session.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Whether or not the current level should be stopped.
        /// </summary>
        public bool ShouldStop => scoreFactors.Any(sf => sf.ShouldStop);
    }
}
