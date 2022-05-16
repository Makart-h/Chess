using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;

namespace Chess.AI
{
    internal abstract class Controller
    {
        private readonly List<Piece> pieces;
        public Piece[] Pieces { get { return pieces.ToArray(); } }
        private readonly King king;
        public static event EventHandler<MoveChosenEventArgs> MoveChosen;

        public Controller(Piece[] pieces)
        {
            foreach (var piece in pieces)
            {
                this.pieces.Add(piece);
                if(piece is King k)
                    king = k;
            }
        }
        public virtual void Update()
        {
            king.ClearThreats();
            king.FindAllThreats();
            foreach (var piece in this.pieces)
                piece.Update();
        }
        public abstract void ChooseAMove();

        protected void OnMoveChosen(MoveChosenEventArgs args)
        {
            MoveChosen?.Invoke(this, args);
        }
    }
}
