namespace Retro_ML.Neural.Train.StopCondition
{
    /// <summary>
    /// Interface to be used by the trainer to determine if training should stop or not.
    /// </summary>
    public interface IStopCondition
    {
        /// <summary>
        /// Name of the stop condition
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of the condition when hovering it
        /// </summary>
        string Tooltip { get; }

        /// <summary>
        /// Whether or not this stop condition should be used.
        /// </summary>
        bool ShouldUse { get; set; }

        /// <summary>
        /// Returns whether or not this stop condition has an additional parameter
        /// </summary>
        bool HasParam { get; }

        /// <summary>
        /// The name of the additional parameter
        /// </summary>
        string ParamName { get; }

        /// <summary>
        /// The value of the additional parameter
        /// </summary>
        int ParamValue { get; set; }

        /// <summary>
        /// To be called when training starts
        /// </summary>
        void Start();

        /// <summary>
        /// Returns whether or not the training should be stopped
        /// </summary>
        /// <param name="stats"></param>
        /// <returns></returns>
        bool CheckShouldStop(TrainingStatistics stats);
    }
}
