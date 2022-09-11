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
        public static event EventHandler<MoveMadeEventArgs> MoveMade;


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
            Chessboard.PieceRemovedFromTheBoard += OnPieceRemovedFromTheBoard;
            Chessboard.PieceAddedToTheBoard += OnPieceAddedToTheBoard;
        }
        public King GetKing(Team team) => King;
        public virtual void Update()
        {
            foreach (var piece in pieces)
                piece.Update();
        }
        public abstract void MakeMove();
        protected virtual void OnMoveMade(MoveMadeEventArgs e)
        {
            MoveMade?.Invoke(this, e);
        }
        protected void OnPieceRemovedFromTheBoard(object sender, PieceEventArgs e)
        {
            pieces.Remove(e.Piece);
        }
        protected void OnPieceAddedToTheBoard(object sender, PieceEventArgs e)
        {
            if(e.Piece.Team == team)
                pieces.Add(e.Piece);
        }
        public bool GetPiece(Square square, out Piece piece)
        {
            return Chessboard.Instance.GetAPiece(square, out piece);
        }
        public Team IsSquareOccupied(Square square)
        {
            return Chessboard.Instance.IsSquareOccupied(square);
        }
        public bool ArePiecesFacingEachOther(Piece first, Piece second)
        {
            return Chessboard.Instance.ArePiecesFacingEachOther(first, second);
        }
        public void OnPromotion(Piece piece)
        {
            Chessboard.Instance.OnPromotion(piece);
        }
    }
}
