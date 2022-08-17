using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;

namespace Chess.AI
{
    internal class NoMovesEventArgs
    {
        public Team Team;
        public bool KingThreatened;

        public NoMovesEventArgs(Team team, bool kingThreatened)
        {
            Team = team;
            KingThreatened = kingThreatened;
        }
    }
}
