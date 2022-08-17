using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;
using Chess.Board;
using System.Threading.Tasks;

namespace Chess.AI
{
    internal abstract class Controller : IPieceOwner
    {
        protected readonly List<Piece> pieces;
        public King King { get; private set; }
        protected readonly Team team;
        public Team Team { get => team; }
        public Piece[] Pieces { get { return pieces.ToArray(); } }
        public static event EventHandler<MoveChosenEventArgs> MoveChosen;
        public static event EventHandler<NoMovesEventArgs> NoMovesAvailable;
        private bool updatePieces;
        private int legalMoves;

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
        public King GetKing(Team team) => King;
        public virtual bool Update()
        {
            if (updatePieces)
            {
                legalMoves = 0;
                foreach (var piece in pieces)
                    legalMoves += piece.Update();
                updatePieces = false;
            }

            if (legalMoves == 0)
            {
                OnNoMovesAvailable(new NoMovesEventArgs(this.team, King.Threatened));
                return false;
            }
            return true;
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
        protected void OnNoMovesAvailable(NoMovesEventArgs args)
        {
            NoMovesAvailable?.Invoke(this, args);
        }
    }
}
