using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;

namespace Chess.AI
{
    internal class AIController : Controller
    {
        public AIController(Piece[] pieces) : base(pieces)
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
