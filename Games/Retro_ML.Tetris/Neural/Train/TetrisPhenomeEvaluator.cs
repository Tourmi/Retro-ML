using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural.Scoring;
using Retro_ML.Neural.Train;
//using Retro_ML.Tetris.Game;
using SharpNeat.BlackBox;

namespace Retro_ML.Tetris.Neural.Train;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class TetrisPhenomeEvaluator : DefaultPhenomeEvaluator
{
    public TetrisPhenomeEvaluator(EmulatorManager emulatorManager, ApplicationConfig appConfig, TetrisTrainer trainer) : base(emulatorManager, appConfig, trainer) { }

    protected override int FrameSkip => 1;
    protected override bool FrameSkipShouldKeepControllerInputs => false;

    protected override void DoSaveState(IBlackBox<double> phenome, Score score, string state)
    {
        emulator!.LoadState(Path.GetFullPath(state));
        WaitThenStart();
        emulator.NextFrame();
        dataFetcher!.NextState();

        DoEvaluationLoop(phenome, score);

        score.LevelDone();
    }

    private void WaitThenStart()
    {
        for (int i = Random.Shared.Next(120); i > 0; i--)
        {
            emulator!.NextFrame();
        }

        var input = appConfig.GetConsolePlugin().GetInput();
        input.FromString("S");
        emulator!.SendInput(input);

        for (int i = 0; i < 8; i++)
        {
            emulator!.NextFrame();
        }
    }
}
