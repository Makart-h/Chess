using Chess.Board;
using Chess.Graphics;
using Chess.Pieces;
using Chess.Pieces.Info;
using System;
using System.Collections.Generic;

namespace Chess.AI;
{
    internal abstract class Controller : IPieceOwner
    {
        protected readonly List<Piece> _pieces;
        protected readonly Team _team;
        public King King { get; private set; }      
        public Team Team { get; init; }
        public Piece[] Pieces { get { return _pieces.ToArray(); } }
        public static event EventHandler<MoveMadeEventArgs> MoveMade;

        public Controller(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant)
        {
            Team = team;
            _pieces = new();
            AddPieces(pieces, castlingRights, enPassant);
            _pieces.Sort();
            SubscribeToEvents();
        }
        private void AddPieces(Piece[] pieces, CastlingRights castlingRights, Square? enPassant)
        {
            foreach (Piece piece in pieces)
            {
                piece.Owner = this;
                if (piece is King k)
                {
                    k.CastlingRights = castlingRights;
                    King = k;
                }
                else if (piece is Pawn p)
                {
                    if (enPassant.HasValue && p.Square == enPassant.Value)
                        p.EnPassant = true;
                }
                _pieces.Add(piece);
            }
        }
        private void SubscribeToEvents()
        {
            Chessboard.PieceRemovedFromTheBoard += OnPieceRemovedFromTheBoard;
            Chessboard.PieceAddedToTheBoard += OnPieceAddedToTheBoard;
        }
        public King GetKing(Team team) => King;
        public virtual void Update()
        {
            foreach (Piece piece in _pieces)
                piece.Update();
        }
        public abstract void MakeMove();
        protected virtual void OnMoveMade(MoveMadeEventArgs e)
        {
            MoveMade?.Invoke(this, e);
        }
        protected void OnPieceRemovedFromTheBoard(object sender, PieceEventArgs e) => _pieces.Remove(e.Piece);
        protected void OnPieceAddedToTheBoard(object sender, PieceEventArgs e)
        {
            if (e.Piece.Team == Team)
                _pieces.Add(e.Piece);
        }
        public bool GetPiece(Square square, out Piece piece) => Chessboard.Instance.GetAPiece(square, out piece);
        public Team IsSquareOccupied(Square square) => Chessboard.Instance.IsSquareOccupied(square);
        public bool ArePiecesFacingEachOther(Piece first, Piece second) => Chessboard.Instance.ArePiecesFacingEachOther(first, second);
        public void OnPromotion(Piece piece) => Chessboard.Instance.OnPromotion(piece);
    }
}
