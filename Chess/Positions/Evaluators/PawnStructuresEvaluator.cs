using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.Positions.Evaluators;

internal sealed class PawnStructuresEvaluator
{
    private readonly Dictionary<char, List<int>> _whitePawns;
    private readonly Dictionary<char, List<int>> _blackPawns;
    private int _kingsideCount;
    private int _queensideCount;
    private int _pawnCounter;
    private static readonly int s_whiteNearPromotionRank = 7;
    private static readonly int s_blackNearPromotionRank = 2;
    private static readonly (char first, char last) s_queensideLetters = ('a', 'd');
    private static readonly char s_lowestPossibleKey = 'a';
    private static readonly char s_highestPossibleKey = 'h';
    private static readonly double s_endgameFactor = 2;
    private static readonly double s_pawnCountAdvantageValue = 0.1;
    private static readonly double s_doubledValue = -0.1;
    private static readonly double s_isolatedValue = -0.1;
    private static readonly double s_passedValue = 0.1;
    private static readonly double s_connectedValue = 0.1;
    private static readonly double s_connectedPassedValue = 0.1;
    private static readonly double s_backwardValue = -0.1;
    private static readonly double s_nearPromotionValue = 3.0;
    private static readonly double s_rankValue = 0.05;

    public PawnStructuresEvaluator()
    {
        _whitePawns = new Dictionary<char, List<int>>();
        _blackPawns = new Dictionary<char, List<int>>();
    }
    public void AddPawn(Pawn pawn)
    {
        char letter = pawn.Square.Letter;
        int digit = pawn.Square.Digit;
        var reference = pawn.Team == Team.White ? _whitePawns : _blackPawns;

        if (reference.TryGetValue(letter, out List<int> value))
            value.Add(digit);
        else
            reference[letter] = new List<int>() { digit };
    }
    public double EvaluatePawnStructures(bool inEndgame)
    {
        double evaluation = 0;
        const double pawnCountAdvantageValue = 0.1;
        evaluation += EvaluatePawns(_whitePawns, _blackPawns, headsTowardsHigherRanks: true);
        evaluation -= EvaluatePawns(_blackPawns, _whitePawns);
        evaluation += Math.Sign(_kingsideCount) * pawnCountAdvantageValue;
        evaluation += Math.Sign(_queensideCount) * pawnCountAdvantageValue;
        if (inEndgame)
            evaluation *= 2;
        return evaluation;
    }
    private double EvaluatePawns(Dictionary<char, List<int>> teamToEvaluate, Dictionary<char, List<int>> otherTeam, bool headsTowardsHigherRanks = false)
    {
        const double doubledPawnValue = -0.1;
        double evaluation = 0;
        int doubledPawnsCount = 0;
        foreach (char key in teamToEvaluate.Keys)
        {
            GetCounts(teamToEvaluate[key].Count, key, ref doubledPawnsCount, headsTowardsHigherRanks);
            foreach (int pawnRank in teamToEvaluate[key])
            {
                bool connected = false, passed = false, backward = false, isolated = false;

                bool leftRankExists = teamToEvaluate.ContainsKey((char)(key - 1));
                bool rightRankExists = teamToEvaluate.ContainsKey((char)(key + 1));

                if (!leftRankExists && !rightRankExists)
                    isolated = true;
                else if (leftRankExists && rightRankExists)
                {
                    connected = true;
                    backward = CheckIfBackward(pawnRank, key, teamToEvaluate, headsTowardsHigherRanks);
                }
                else
                    connected = true;
                passed = CheckIfPassed(pawnRank, key, otherTeam, headsTowardsHigherRanks);
                evaluation += EvaluatePawn(pawnRank, connected, passed, backward, isolated, headsTowardsHigherRanks);
            }
        }
        evaluation += doubledPawnsCount * doubledPawnValue;
        return evaluation;
    }
    private static double EvaluatePawn(int pawnRank, bool connected, bool passed, bool backward, bool isolated, bool headsTowardsHigherRanks)
    {
        double evaluation = 0;
        const double isolatedValue = -0.1;
        const double passedValue = 0.1;
        const double connectedValue = 0.1;
        const double connectedPassedValue = 0.1;
        const double backwardValue = -0.1;
        const double nearPromotionValue = 3.0;
        const double rankValue = 0.05;
        int nearPromotionRank = headsTowardsHigherRanks ? 7 : 2;

        if (pawnRank == nearPromotionRank)
            evaluation += nearPromotionValue;
        else
        {
            int rank = headsTowardsHigherRanks ? pawnRank - 2 : 7 - pawnRank;
            evaluation += rankValue * rank;
        }
        if (passed)
            evaluation += passedValue;
        if (isolated)
            evaluation += isolatedValue;
        else
        {
            if (backward)
                evaluation += backwardValue;
            if (connected)
                evaluation += connectedValue;
            if (connected && passed)
                evaluation += connectedPassedValue;
        }
        return evaluation;
    }
    private static bool CheckIfPassed(int pawnRank, char pawnKey, Dictionary<char, List<int>> otherTeam, bool headsTowardsHigherRanks)
    {
        char[] keys = { pawnKey, (char)(pawnKey - 1), (char)(pawnKey + 1) };
        foreach (char key in keys)
        {
            if (otherTeam.TryGetValue(key, out var pawnRanks))
            {
                foreach (int enemy in pawnRanks)
                {
                    if (headsTowardsHigherRanks && enemy > pawnRank || !headsTowardsHigherRanks && enemy < pawnRank)
                        return false;
                }
            }
        }
        return true;
    }
    private static bool CheckIfBackward(int pawnRank, char key, Dictionary<char, List<int>> teamToEvaluate, bool headsTowardsHigherRanks)
    {
        bool backward = true;
        foreach (int otherPawn in teamToEvaluate[(char)(key - 1)])
        {
            if (headsTowardsHigherRanks && pawnRank >= otherPawn || !headsTowardsHigherRanks && pawnRank <= otherPawn)
            {
                backward = false;
                break;
            }
        }
        if (backward == true)
        {
            foreach (int otherPawn in teamToEvaluate[(char)(key + 1)])
            {
                if (headsTowardsHigherRanks && pawnRank >= otherPawn || !headsTowardsHigherRanks && pawnRank <= otherPawn)
                {
                    backward = false;
                    break;
                }
            }
        }
        return backward;
    }
    private void GetCounts(int count, char key, ref int doubledPawnsCount, bool headsTowardsHigherRanks)
    {
        if (count > 1)
            doubledPawnsCount += count - 1;
        if (key >= 'a' && key <= 'd')
            _queensideCount += headsTowardsHigherRanks ? count : -count;
        else
            _kingsideCount += headsTowardsHigherRanks ? count : -count;
    }
}
