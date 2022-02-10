using SharpNeat.BlackBox;
using SharpNeat.Evaluation;
using SharpNeat.Graphs;
using SharpNeat.Graphs.Acyclic;
using SMW_ML.Emulator;
using SMW_ML.Game.SuperMarioWorld;
using SMW_ML.Neural.Scoring;
using System;
using static SMW_ML.Utils.ReflectionTool;

namespace SMW_ML.Neural.Training.SharpNeatImpl
{
    /// <summary>
    /// This class takes care of the evaluation of a single AI.
    /// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
    /// </summary>
    internal class SMWPhenomeEvaluator : IPhenomeEvaluator<IBlackBox<double>>
    {
        private readonly EmulatorManager emulatorManager;
        private IEmulatorAdapter? emulator;
        private DataFetcher? dataFetcher;
        private InputSetter? inputSetter;
        private OutputGetter? outputGetter;

        public SMWPhenomeEvaluator(EmulatorManager emulatorManager)
        {
            this.emulatorManager = emulatorManager;
        }

        public FitnessInfo Evaluate(IBlackBox<double> phenome)
        {
            Score score = new();

            emulator = emulatorManager.WaitOne();
            int[] outputMap = new int[phenome.OutputCount];
            Array.Copy(phenome.OutputVector.GetField<int[]>("_map"), outputMap, phenome.OutputCount);
            emulator.NetworkChanged(GetConnectionLayers(phenome), outputMap);
            dataFetcher = emulator.GetDataFetcher();
            inputSetter = emulator.GetInputSetter();
            outputGetter = emulator.GetOutputGetter();

            var saveStates = emulator.GetStates();
            foreach (var state in saveStates)
            {
                if (!state.Contains("yoshi-island")) continue;
                emulator.LoadState(state);
                emulator.NextFrame();
                dataFetcher.NextLevel();

                while (!score.ShouldStop)
                {
                    DoFrame(phenome);

                    score.Update(dataFetcher);
                    dataFetcher.NextFrame();
                    emulator.NetworkUpdated(VectorToArray(phenome.InputVector), VectorToArray(phenome.OutputVector));
                }
                score.LevelDone();
            }

            dataFetcher = null;
            inputSetter = null;
            outputGetter = null;
            emulatorManager.FreeOne(emulator);
            emulator = null;

            return new FitnessInfo(score.GetFinalScore());
        }

        /// <summary>
        /// Runs a single frame on the emulator, using the given AI
        /// </summary>
        /// <param name="phenome"></param>
        private void DoFrame(IBlackBox<double> phenome)
        {
            phenome.ResetState();
            inputSetter!.SetInputs(phenome.InputVector);

            phenome.Activate();

            emulator!.SendInput(outputGetter!.GetControllerInput(phenome.OutputVector));
            emulator!.NextFrame();
        }

        /// <summary>
        /// Returns an array equivalent to the given SharpNeat Vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private static double[] VectorToArray(IVector<double> vector)
        {
            double[] result = new double[vector.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = vector[i];
            }

            return result;
        }

        /// <summary>
        /// Returns the connections separated by layers of the phenome
        /// </summary>
        /// <param name="phenome"></param>
        /// <returns></returns>
        private static (int sourceNode, int targetNode, double weight)[][] GetConnectionLayers(IBlackBox<double> phenome)
        {
            ConnectionIds connectionIds = phenome.GetField<ConnectionIds>("_connIds");
            double[] weights = phenome.GetField<double[]>("_weightArr");
            LayerInfo[] layerInfos = phenome.GetField<LayerInfo[]>("_layerInfoArr");

            var result = new (int sourceNode, int targetNode, double weight)[layerInfos.Length][];
            int currIndex = 0;
            for (int i = 0; i < result.Length; i++)
            {
                var layerInfo = layerInfos[i];
                result[i] = new (int sourceNode, int targetNode, double weight)[layerInfo.EndConnectionIdx - currIndex];
                int layerIndex = currIndex;
                for (; currIndex < layerInfo.EndConnectionIdx; currIndex++)
                {
                    result[i][currIndex - layerIndex] = (connectionIds.GetSourceId(currIndex), connectionIds.GetTargetId(currIndex), weights[currIndex]);
                }
            }

            return result;
        }
    }
}
