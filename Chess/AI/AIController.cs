using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;
using Chess.Board;

namespace Chess.AI
{
    internal class AIController : Controller
    {
        public AIController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
        {

        }
        public override void Update()
        {
            base.Update();
        }
        public override void ChooseAMove()
        {
            throw new NotImplementedException();
        }
    }
}
