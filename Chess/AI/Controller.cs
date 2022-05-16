using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;

namespace Chess.AI
{
    internal abstract class Controller
    {
        protected readonly List<Piece> pieces;
        protected readonly Team team;
        public Piece[] Pieces { get { return pieces.ToArray(); } }
        public static event EventHandler<MoveChosenEventArgs> MoveChosen;

        public Controller(Team team, Piece[] pieces, CastlingRights castlingRights)
        {
            this.team = team;
            this.pieces = new List<Piece>();
            foreach (var piece in pieces)
            {
                piece.Owner = this;
                if (piece is King k)
                    k.CastlingRights = castlingRights;
                this.pieces.Add(piece);
            }
            this.pieces.Sort();       
        }
        public virtual void Update()
        {
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
