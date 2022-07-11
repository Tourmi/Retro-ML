﻿using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Train;
using SharpNeat.BlackBox;
using SharpNeat.Evaluation;

namespace Retro_ML.SuperBomberman3.Neural.Train;

internal class SB3EvaluationScheme : IBlackBoxEvaluationScheme<double>
{
    private readonly EmulatorManager emulatorManager;
    private readonly ApplicationConfig appConfig;
    private readonly SB3Trainer trainer;

    public int InputCount => appConfig.NeuralConfig.GetInputCount();

    public int OutputCount => appConfig.NeuralConfig.GetOutputCount();

    public bool IsDeterministic => true; // Change if using random levels

    public IComparer<FitnessInfo> FitnessComparer => PrimaryFitnessInfoComparer.Singleton;

    public FitnessInfo NullFitness => FitnessInfo.DefaultFitnessInfo;

    //We need an emulator instance for every AI.
    public bool EvaluatorsHaveState => true;

    public SB3EvaluationScheme(EmulatorManager emulatorManager, ApplicationConfig appConfig, SB3Trainer trainer)
    {
        this.emulatorManager = emulatorManager;
        this.appConfig = appConfig;
        this.trainer = trainer;
    }

    public IPhenomeEvaluator<IBlackBox<double>> CreateEvaluator() => new SharpNeatPhenomeEvaluator(emulatorManager, appConfig, trainer);

    public bool TestForStopCondition(FitnessInfo fitnessInfo) => false;
}
