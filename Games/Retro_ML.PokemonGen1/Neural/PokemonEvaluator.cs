﻿using Retro_ML.Configuration;
using Retro_ML.Emulator;
using Retro_ML.Neural;
using Retro_ML.Neural.Scoring;
using Retro_ML.PokemonGen1.Game;
using SharpNeat.BlackBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Retro_ML.PokemonGen1.Neural;
internal class PokemonEvaluator : DefaultEvaluator
{
    private List<double> movesScores;
    private int selectedMove;
    private PokemonDataFetcher df;
    private const int MAX_POKEMON_ID = 191;

    public PokemonEvaluator(ApplicationConfig appConfig, IBlackBox<double> phenome, IEnumerable<string> saveStates, IEmulatorAdapter emulator) : base(appConfig, phenome, saveStates, emulator)
    {
        movesScores = new List<double>();
        df = (PokemonDataFetcher)dataFetcher;
    }

    protected override void DoSaveState(IBlackBox<double> phenome, Score score, string state)
    {
        emulator.LoadState(Path.GetFullPath(state));

        dataFetcher.NextState();

        WriteRandomEncounterAddresses();

        emulator.NextFrame();

        DoEvaluationLoop(phenome, score);

        score.LevelDone();
    }

    protected override void DoActivation(IBlackBox<double> blackBox)
    {
        movesScores.Clear();

        //Open attack menu
        //Select move 1
        PressA(10);
        while (selectedMove > 0)
        {
            PressUp(10);
            selectedMove--;
        }

        AddMoveScore(blackBox);

        if (((PokemonDataFetcher)dataFetcher).Move2Exists())
        {
            //Select move 2
            PressDown(10);
            AddMoveScore(blackBox);
        }

        if (((PokemonDataFetcher)dataFetcher).Move3Exists())
        {
            //Select move 3
            PressDown(10);
            AddMoveScore(blackBox);
        }

        if (((PokemonDataFetcher)dataFetcher).Move4Exists())
        {
            //Select move 4
            PressDown(10);
            AddMoveScore(blackBox);
        }

        //Go back to first move
        PressDown(10);
    }

    protected override void DoAIAction(IBlackBox<double> phenome)
    {
        selectedMove = movesScores.ToList().IndexOf(movesScores.Max());

        //use move with best score
        int index = 0;
        while (index != selectedMove)
        {
            PressDown(10);
            index++;
        }
        PressA(4);

        //wait until turn over
        SkipThroughTurn();
    }

    private void SkipThroughTurn()
    {
        while (!df.IsFightOptionSelected() && !df.LostFight() && !df.WonFight())
        {
            PressB(30, hold: true);
            emulator.NextFrame();
        }
        emulator.NextFrames(5, false);
    }

    private void Press(string inputStr, int waitAfter = 1, bool hold = false)
    {
        var input = appConfig.GetConsolePlugin().GetInput();
        input.FromString(inputStr);
        emulator.SendInput(input);
        emulator.NextFrames(waitAfter, hold);
    }

    private void PressA(int waitAfter = 1, bool hold = false) => Press("A", waitAfter, hold);

    private void PressB(int waitAfter = 1, bool hold = false) => Press("B", waitAfter, hold);

    private void PressDown(int waitAfter = 1, bool hold = false) => Press("d", waitAfter, hold);

    private void PressUp(int waitAfter = 1, bool hold = false) => Press("u", waitAfter, hold);

    private void AddMoveScore(IBlackBox<double> blackBox)
    {
        if (df.GetMoveCurrentPP(movesScores.Count) == 0)
        {
            movesScores.Add(double.NegativeInfinity);
            return;
        }
        blackBox!.ResetState();
        inputSetter.SetInputs(blackBox.InputVector);
        blackBox.Activate();

        movesScores.Add(blackBox.OutputVector[0]);
    }

    private byte GetRandomPokemonID()
    {
        int[] missingNO = new int[] { 31, 32, 50, 52, 56, 61, 62, 63, 67, 68,
                                      69, 79, 80, 81, 86, 87, 94, 95, 115, 121,
                                      122, 127, 134, 135, 137, 140, 146, 156, 159,
                                      160, 161, 162, 172, 174, 175, 181, 182, 183, 184 };
        Random rdm = new Random();
        byte randID = 0;
        do
        {
            randID = (byte)rdm.Next(1, MAX_POKEMON_ID);
        } while (missingNO.Contains(randID));

        return randID;
    }

    private void WriteRandomEncounterAddresses()
    {
        int yellowOffset = df.IsPokemonYellow ? -1 : 0;
        byte randomPokemonID = GetRandomPokemonID();
        for (int i = 0; i < 20; i += 2)
        {
            emulator.WriteMemory((uint)(Addresses.WildEncounters.Encounter1.Address + i + yellowOffset), randomPokemonID);
        }
    }

}
