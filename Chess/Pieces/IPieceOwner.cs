using System;
using System.Collections.Generic;
using System.Text;

namespace Chess.Pieces
{
    internal interface IPieceOwner
    {
        public King GetKing(Team team);
    }
}
