using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;

namespace Chess.AI
{
    internal class HumanController : Controller
    {
        public HumanController(Piece[] pieces) : base(pieces)
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
