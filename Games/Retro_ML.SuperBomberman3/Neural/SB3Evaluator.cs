using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;

namespace Retro_ML.SuperBomberMan3.Neural;

/// <summary>
/// This class takes care of the evaluation of a single AI.
/// Since it has an internal state, it may not be used to evaluate multiple AIs at once on a single instance.
/// </summary>
internal class SB3Evaluator : BaseEvaluator
{
    public SB3Evaluator(ApplicationConfig appConfig, IPhenomeWrapper phenome, IEnumerable<string> saveStates, EmulatorManager emulatorManager) : base(appConfig, phenome, saveStates, emulatorManager) { }
    protected override void DoSaveState(Score score, string state)
    {
        emulator!.LoadState(Path.GetFullPath(state));
        WaitThenStart();
        emulator.NextFrame();
        dataFetcher!.NextState();

        DoEvaluationLoop(score);

        score.LevelDone();
    }

    private void WaitThenStart()
    {
        for (int i = Random.Shared.Next(120); i > 0; i--)
        {
            emulator!.NextFrame();
        }

        var input = appConfig.GetConsolePlugin().GetInput();
        input.FromString("A");
        emulator!.SendInput(input);

        for (int i = 0; i < 8; i++)
        {
            emulator!.NextFrame();
        }
    }
}
