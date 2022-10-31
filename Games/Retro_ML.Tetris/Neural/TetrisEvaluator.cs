﻿using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.Tetris.Configuration;
using Retro_ML.Tetris.Game;
using SharpNeat.BlackBox;

namespace Retro_ML.Tetris.Neural;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class TetrisEvaluator : DefaultEvaluator
{
    public TetrisEvaluator(ApplicationConfig appConfig, IBlackBox<double> phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) : base(appConfig, phenome, saveStates, emulator) { }

    protected override int FrameSkip => ((TetrisPluginConfig)appConfig.GamePluginConfig!).FrameSkip;
    protected override bool FrameSkipShouldKeepControllerInputs => false;

    protected override void DoSaveState(IBlackBox<double> phenome, Score score, string state)
    {
        for (int i = 0; i < ((TetrisPluginConfig)appConfig.GamePluginConfig!).NbAttempts; i++)
        {
            emulator!.LoadState(Path.GetFullPath(state));

            if (ShouldStop)
            {
                return;
            }
            WaitThenStart();
            emulator.NextFrame();
            dataFetcher!.NextState();

            DoEvaluationLoop(phenome, score);

            score.LevelDone();
        }
    }

    protected override void DoAIAction(IBlackBox<double> phenome)
    {
        if (((TetrisPluginConfig)appConfig.GamePluginConfig!).UseControllerOutput)
        {
            base.DoAIAction(phenome);
        }
        else
        {
            RotatePiece(phenome.OutputVector);
            MovePiece(phenome.OutputVector);
            PlacePiece();
        }
    }

    private void WaitThenStart()
    {
        for (int i = Random.Shared.Next(120); i > 0; i--)
        {
            emulator!.NextFrame();
        }

        PressButton("S", 1);

        for (int i = 0; i < 7; i++)
        {
            emulator!.NextFrame();
        }
    }

    private void RotatePiece(IVector<double> outputs)
    {
        int bestRotationIndex = 0;
        double bestRotationValue = double.NegativeInfinity;

        for (int i = TetrisDataFetcher.PLAY_WIDTH; i < TetrisDataFetcher.PLAY_WIDTH + 4; i++)
        {
            if (outputs[i] > bestRotationValue)
            {
                bestRotationIndex = i - TetrisDataFetcher.PLAY_WIDTH;
                bestRotationValue = outputs[i];
            }
        }

        while (bestRotationIndex > 0)
        {
            PressButton("A", 1);
            emulator.NextFrame();
            bestRotationIndex--;
        }
    }

    private void MovePiece(IVector<double> outputs)
    {
        int bestLocationIndex = 0;
        double bestLocationValue = double.NegativeInfinity;

        for (int i = 0; i < TetrisDataFetcher.PLAY_WIDTH; i++)
        {
            if (outputs[i] > bestLocationValue)
            {
                bestLocationValue = outputs[i];
                bestLocationIndex = i;
            }
        }

        while (bestLocationIndex < TetrisDataFetcher.INITIAL_X_INDEX)
        {
            PressButton("l", 1);
            emulator.NextFrame();
            bestLocationIndex++;
        }

        while (bestLocationIndex > TetrisDataFetcher.INITIAL_X_INDEX)
        {
            PressButton("r", 1);
            emulator.NextFrame();
            bestLocationIndex--;
        }
    }

    private void PlacePiece()
    {
        TetrisDataFetcher df = (TetrisDataFetcher)dataFetcher;
        int oldY = df.GetPositionYIndex();
        while (!df.IsGameOver() && df.GetPositionYIndex() >= oldY)
        {
            PressButton("d", 1);
            emulator.NextFrame();

            oldY = df.GetPositionYIndex();
            df.NextFrame();
        }
    }

    private void PressButton(string button, int framesToHold)
    {
        var input = appConfig.GetConsolePlugin().GetInput();
        input.FromString(button);
        emulator.SendInput(input);
        emulator.NextFrames(framesToHold, true);
    }
}
