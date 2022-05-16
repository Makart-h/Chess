using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;
using Chess.Board;

namespace Chess.AI
{
    internal abstract class Controller
    {
        protected readonly List<Piece> pieces;
        public King King { get; private set; }
        protected readonly Team team;
        public Piece[] Pieces { get { return pieces.ToArray(); } }
        public static event EventHandler<MoveChosenEventArgs> MoveChosen;
        private bool updatePieces;

        public Controller(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant)
        {
            this.team = team;
            this.pieces = new List<Piece>();
            foreach (var piece in pieces)
            {
                piece.Owner = this;
                if (piece is King k)
                {
                    k.CastlingRights = castlingRights;
                    King = k;
                }
                if (enPassant.HasValue)
                    if (piece is Pawn p && p.Square == enPassant.Value)
                        p.EnPassant = true;
                this.pieces.Add(piece);
            }
            this.pieces.Sort();
            updatePieces = true;
            Chessboard.PieceRemovedFromTheBoard += OnPieceRomovedFromTheBoard;
        }
        public virtual void Update()
        {
            if (updatePieces)
            {
                foreach (var piece in pieces)
                    piece.Update();
                updatePieces = false;
            }
        }
        public abstract void ChooseAMove();

        protected void OnMoveChosen(MoveChosenEventArgs args)
        {
            updatePieces = true;
            MoveChosen?.Invoke(this, args);
        }
        protected void OnPieceRomovedFromTheBoard(object sender, PieceRemovedFromTheBoardEventArgs args)
        {
            pieces.Remove(args.Piece);
        }
    }
}
