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
        _pawnCounter = 0;
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

        _pawnCounter++;
    }
    public double EvaluatePawnStructures(bool inEndgame)
    {
        double evaluation = 0;
        if (_pawnCounter == 0)
            return evaluation;
        evaluation += EvaluatePawns(_whitePawns, _blackPawns, PawnDetails.HeadsTowardsHigherRanks);
        evaluation -= EvaluatePawns(_blackPawns, _whitePawns);
        evaluation += Math.Sign(_kingsideCount) * s_pawnCountAdvantageValue;
        evaluation += Math.Sign(_queensideCount) * s_pawnCountAdvantageValue;
        if (inEndgame)
            evaluation *= s_endgameFactor;
        return evaluation;
    }
    private double EvaluatePawns(Dictionary<char, List<int>> teamToEvaluate, Dictionary<char, List<int>> otherTeam, PawnDetails assumptions = PawnDetails.None)
    {
        double evaluation = 0;
        int doubledPawnsCount = 0;
        for (char key = s_lowestPossibleKey; key < s_highestPossibleKey + 1; key++)
        {
            if (!teamToEvaluate.ContainsKey(key))
                continue;
            GetCounts(teamToEvaluate[key].Count, key, ref doubledPawnsCount, assumptions);
            foreach (int pawnRank in teamToEvaluate[key])
            {
                PawnDetails pawnDetails = assumptions;

                bool leftRankExists = teamToEvaluate.ContainsKey((char)(key - 1));
                bool rightRankExists = teamToEvaluate.ContainsKey((char)(key + 1));

                if (!leftRankExists && !rightRankExists)
                    pawnDetails |= PawnDetails.Isolated;
                else if (leftRankExists && rightRankExists)
                {
                    pawnDetails |= PawnDetails.Connected;
                    if (CheckIfBackward(pawnRank, key, teamToEvaluate, pawnDetails))
                        pawnDetails |= PawnDetails.Backward;
                }
                else
                    pawnDetails |= PawnDetails.Connected;

                if (CheckIfPassed(pawnRank, key, otherTeam, pawnDetails))
                    pawnDetails |= PawnDetails.Passed;

                evaluation += EvaluatePawn(pawnRank, pawnDetails);
            }
        }
        evaluation += doubledPawnsCount * s_doubledValue;
        return evaluation;
    }
    private static double EvaluatePawn(int pawnRank, PawnDetails pawnDetails)
    {
        double evaluation = 0;
        bool headsTowardsHigherRanks = (pawnDetails & PawnDetails.HeadsTowardsHigherRanks) != 0;
        int nearPromotionRank = headsTowardsHigherRanks ? s_whiteNearPromotionRank : s_blackNearPromotionRank;

        if (pawnRank == nearPromotionRank)
            evaluation += s_nearPromotionValue;
        else
        {
            int rank = headsTowardsHigherRanks ? pawnRank - s_blackNearPromotionRank : s_whiteNearPromotionRank - pawnRank;
            evaluation += s_rankValue * rank;
        }
        double passed = (double)(pawnDetails & PawnDetails.Passed) / (double)PawnDetails.Passed;
        evaluation += s_passedValue * passed;
        evaluation += s_isolatedValue * (double)(pawnDetails & PawnDetails.Isolated) / (double)PawnDetails.Isolated;
        evaluation += s_backwardValue * (double)(pawnDetails & PawnDetails.Backward) / (double)PawnDetails.Backward;
        double connected = (double)(pawnDetails & PawnDetails.Connected) / (double)PawnDetails.Connected;
        evaluation += s_connectedValue * connected;
        evaluation += s_connectedPassedValue * connected * passed;
        return evaluation;
    }
    private static bool CheckIfPassed(int pawnRank, char pawnKey, Dictionary<char, List<int>> otherTeam, PawnDetails pawnDetails)
    {
        char[] keys = { pawnKey, (char)(pawnKey - 1), (char)(pawnKey + 1) };
        bool headsTowardsHigherRanks = (pawnDetails & PawnDetails.HeadsTowardsHigherRanks) != 0;
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
    private static bool CheckIfBackward(int pawnRank, char key, Dictionary<char, List<int>> teamToEvaluate, PawnDetails pawnDetails)
    {
        bool backward = true;
        bool headsTowardsHigherRanks = (pawnDetails & PawnDetails.HeadsTowardsHigherRanks) != 0;
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
    private void GetCounts(int count, char key, ref int doubledPawnsCount, PawnDetails pawnDetails)
    {
        bool headsTowardsHigherRanks = (pawnDetails & PawnDetails.HeadsTowardsHigherRanks) != 0;
        if (count > 1)
            doubledPawnsCount += count - 1;
        if (key >= s_queensideLetters.first && key <= s_queensideLetters.last)
            _queensideCount += headsTowardsHigherRanks ? count : -count;
        else
            _kingsideCount += headsTowardsHigherRanks ? count : -count;
    }
}
