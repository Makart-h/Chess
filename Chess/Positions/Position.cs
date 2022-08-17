using System;
using System.Collections.Generic;
using Chess.Pieces;
using Chess.Movement;
using Chess.Board;
using System.Linq;
using System.Threading.Tasks;

namespace Chess.Positions
{
    internal class Position : IPieceOwner
    {
        private readonly Dictionary<Square, Piece> pieces;
        private readonly Team activeTeam;
        private readonly int halfMoves;
        private King white;
        private King black;
        public Dictionary<Square, Piece> Pieces { get => pieces; }
        public Team ActiveTeam { get => activeTeam; }
        public int HalfMoves { get => halfMoves; }
        public Position()
        {
            pieces = new Dictionary<Square, Piece>(Chessboard.NumberOfSquares*Chessboard.NumberOfSquares);

            foreach (var letter in Enumerable.Range('a', Chessboard.NumberOfSquares).Select(n => (char)n))
            {
                foreach (var number in from n in Enumerable.Range(1, Chessboard.NumberOfSquares)
                                       select int.Parse(n.ToString()))
                {
                    pieces[new Square(letter, number)] = null;
                }
            }
        }

        public Position(Position other, Move move) : this()
        {
            activeTeam = other.activeTeam == Team.White ? Team.Black : Team.White;
            halfMoves = other.halfMoves;
            Parallel.ForEach(other.pieces.Keys, 
                square => {
                    if (other.pieces[square] != null)
                    {
                        pieces[square] = PieceFactory.CopyAPiece(other.pieces[square], this, true);
                        if (pieces[square] is King k)
                        {
                            if (k.Team == Team.White)
                                white = k;
                            else
                                black = k;
                        }
                    }
                });
            ApplyMove(move);
            Update();
        }
        public Position(Chessboard board, Team activeTeam, Move move, int halfMoves = 0) : this()
        {
            this.activeTeam = activeTeam;
            this.halfMoves = halfMoves;
   
            foreach (var key in board.Pieces.Keys)
            {
                if (board.Pieces[key] != null)
                {
                    pieces[key] = PieceFactory.CopyAPiece(board.Pieces[key], this, true);
                    if (pieces[key] is King k)
                    {
                        if (k.Team == Team.White)
                            white = k;
                        else
                            black = k;
                    }
                }
            }
            ApplyMove(move);
            Update();
        }
        public King GetKing(Team team)
        {
            return team switch
            {
                Team.White => white,
                Team.Black => black,
                _ => null
            };
        }
        private void ApplyMove(Move move)
        {
            move = pieces[move.Former]?.GetAMove(move.Latter);
            if (move != null)
            {              
                pieces[move.Latter] = pieces[move.Former];
                pieces[move.Former] = null;
                if (move.Description == "en passant")
                {
                    Square enPassant = new Square(move.Latter.Number.letter, move.Former.Number.digit);
                    pieces[enPassant] = null;
                }
                else if (move.Description.Contains("castle"))
                {
                    int direction = move.Former.Number.letter > move.Latter.Number.letter ? 1 : -1;
                    string square = move.Description.Split(':')[1];
                    Square originalRookPosition = new Square(square[0], int.Parse(square[1].ToString()));
                    Square newRookPosition = new Square((char)(move.Latter.Number.letter + direction), move.Latter.Number.digit);
                    Move rookMove = new Move(pieces[originalRookPosition].Square, newRookPosition, "castle");
                    pieces[originalRookPosition].MovePiece(rookMove);
                    pieces[newRookPosition] = pieces[originalRookPosition];
                    pieces[originalRookPosition] = null;

                }
                pieces[move.Latter].MovePiece(move);
            }
            else
                throw new InvalidOperationException("No such move on the given piece!");
        }
        private void Update()
        {
            white.Update();
            black.Update();
            foreach (var piece in pieces.Values)
            {
                if (piece is King)
                    continue;
                piece?.Update();
            }
        }
    }
}
