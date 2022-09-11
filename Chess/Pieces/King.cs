﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;
using System.Linq;

namespace Chess.Pieces
{
    class King : Piece
    {
        public CastlingRights CastlingRights { get; set; }
        private readonly static Dictionary<char, Square> castlingRookSquares;
        private readonly static Dictionary<Team, Square[]> kingsideCastlingSquares;
        private readonly static Dictionary<Team, Square[]> queensideCastlingSquares;
        private readonly List<Square[]> threats;
        public static event EventHandler Check;
        public bool Threatened { get => threats.Count > 0; }
        static King()
        {
            castlingRookSquares = new Dictionary<char, Square>();
            castlingRookSquares['K'] = new Square("H1");
            castlingRookSquares['Q'] = new Square("A1");
            castlingRookSquares['k'] = new Square("H8");
            castlingRookSquares['q'] = new Square("A8");

            kingsideCastlingSquares = new Dictionary<Team, Square[]>();
            queensideCastlingSquares = new Dictionary<Team, Square[]>();

            kingsideCastlingSquares[Team.White] = new[] { new Square("F1"), new Square("G1")};
            kingsideCastlingSquares[Team.Black] = new[] { new Square("F8"), new Square("G8")};
            queensideCastlingSquares[Team.White] = new[] { new Square("B1"), new Square("C1"), new Square("D1") };
            queensideCastlingSquares[Team.Black] = new[] { new Square("B8"), new Square("C8"), new Square("D8") };

        }
        public King(Team team, Square square, Texture2D rawTexture, bool isRaw = false) : base(team, square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.King, Square.SquareHeight * ((byte)team & 1), Square.SquareWidth, Square.SquareHeight);
            moves = new List<Move>();
            threats = new List<Square[]>();
            moveSet = MoveSets.King;
            CastlingRights = CastlingRights.None;
            Value = 0;
        }
        public King(King other, bool isRaw = false) : base(other.team, other.Square)
        {
            IsRawPiece = isRaw;
            model = IsRawPiece ? null : other.model;
            moves = other.CopyMoves();
            threats = other.CopyThreats();
            CastlingRights = other.CastlingRights;
            Value = other.Value;
            moveSet = other.MoveSet;
        }
        public override int Update()
        {          
            FindAllThreats();
            return base.Update();
        }
        private List<Square[]> CopyThreats()
        {
            List<Square[]> copy = new List<Square[]>();
            foreach(var threat in threats)
            {
                copy.Add(threat);
            }
            return copy;
        }
        public static Square GetCastlingRookSquare(char sideOfCastling, Team team)
        {
            return (team, sideOfCastling) switch
            {
                (Team.White, 'k') => castlingRookSquares['K'],
                (Team.White, 'q') => castlingRookSquares['Q'],
                (Team.Black, 'k') => castlingRookSquares['k'],
                (Team.Black, 'q') => castlingRookSquares['q'],
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public static char GetCastlingSideFromRookSquare(Square square, Team team)
        {
            string squareAsString = square.ToString();
            return (team, squareAsString) switch
            {
                (Team.White, "H1") => 'k',
                (Team.White, "A1") => 'q',
                (Team.Black, "H8") => 'k',
                (Team.Black, "A8") => 'q',
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        public override void CheckPossibleMoves()
        {
            CheckRegularMoves();
            CheckCastlingMoves();          
        }
        private void CheckRegularMoves()
        {
            foreach (var square in GetSquaresAroundTheKing())
            {
                if (!Square.Validate(square))
                    continue;

                Team teamOnTheSquare = Owner.IsSquareOccupied(square);

                if (teamOnTheSquare == team || CheckIfSquareIsThreatened(square))
                    continue;

                if (teamOnTheSquare == Team.Empty)
                {
                    moves.Add(new Move(Square, square, 'm'));
                }
                else
                {
                    moves.Add(new Move(Square, square, 'x'));
                }
            }
        }
        public void CheckCastlingMoves()
        {
            if (threats.Count == 0)
            {
                if ((CastlingRights & CastlingRights.KingSide) == CastlingRights.KingSide)
                {
                    char kingSideLetter = team == Team.White ? 'K' : 'k';
                    if (Owner.GetPiece(castlingRookSquares[kingSideLetter], out Piece piece))
                        CheckCastling(GetKingSideCastleSquares(), piece);
                }
                if ((CastlingRights & CastlingRights.QueenSide) == CastlingRights.QueenSide)
                {
                    char queenSideLetter = team == Team.White ? 'Q' : 'q';
                    if (Owner.GetPiece(castlingRookSquares[queenSideLetter], out Piece piece))
                        CheckCastling(GetQueenSideCastleSquares(), piece);
                }
            }
        }
        private void CheckCastling(Square[] squaresToCheck, Piece potentialRook)
        {
            if (!(potentialRook is Rook r) || r.HasMoved)
            {
                if (squaresToCheck[^1] == GetKingSideCastleSquares()[^1])
                {
                    if ((CastlingRights & CastlingRights.QueenSide) != 0)
                        CastlingRights = CastlingRights.QueenSide;
                    else
                        CastlingRights = CastlingRights.None;
                }
                else
                {
                    if ((CastlingRights & CastlingRights.KingSide) != 0)
                        CastlingRights = CastlingRights.KingSide;
                    else
                        CastlingRights = CastlingRights.None;
                }       
                return;
            }

            foreach(var square in squaresToCheck)
            {
                Team teamOnTheSquare = Owner.IsSquareOccupied(square);
                if (teamOnTheSquare != Team.Empty || CheckIfSquareIsThreatened(square))
                    return;
            }

            moves.Add(new Move(Square, squaresToCheck[1], GetCastlingSideFromRookSquare(r.Square, team)));
        }
        public Square[] GetSquaresAroundTheKing()
        {
            return new Square[] {
                new Square((char)(Square.Number.letter + 1), Square.Number.digit + 1),
                new Square((char)(Square.Number.letter + 1), Square.Number.digit - 1),
                new Square((char)(Square.Number.letter - 1), Square.Number.digit + 1),
                new Square((char)(Square.Number.letter - 1), Square.Number.digit - 1),
                new Square((char)(Square.Number.letter + 1), Square.Number.digit),
                new Square((char)(Square.Number.letter - 1), Square.Number.digit),
                new Square(Square.Number.letter, Square.Number.digit + 1),
                new Square(Square.Number.letter, Square.Number.digit - 1),
            };
        }
        public Square[] GetKingSideCastleSquares() => kingsideCastlingSquares[team];
        public Square[] GetQueenSideCastleSquares() => queensideCastlingSquares[team];
        public bool CheckMoveAgainstThreats(Piece piece, Move move) => !WouldTheMoveGenerateAThreat(piece, move) && WouldTheMoveSolveAllTheThreats(move);
        public bool WouldTheMoveGenerateAThreat(Piece piece, Move move)
        {
            Move[] moves;
            if (!Owner.ArePiecesFacingEachOther(this, piece))
                return false;

            int x = move.Former.Number.letter - Square.Number.letter;
            int y = move.Former.Number.digit - Square.Number.digit;
            if(x == 0 && move.Former.Number.letter != move.Latter.Number.letter)
            {
                y /= Math.Abs(y);
                moves = Move.GenerateMovesInADirection(piece.Square, (0, y), Owner, excludeMove:"moves");
                if (moves.Length == 0)
                    return false;
                Owner.GetPiece(moves[^1].Latter, out Piece attackingPiece);
                if (((attackingPiece?.MoveSet & MoveSets.Rook) != 0) && attackingPiece.Square != move.Latter)
                    return true;
                else
                    return false;
            }
            else if(y == 0 && move.Former.Number.digit != move.Latter.Number.digit)
            {
                x /= Math.Abs(x);
                moves = Move.GenerateMovesInADirection(piece.Square, (x, 0), Owner, excludeMove: "moves");
                if (moves.Length == 0)
                    return false;
                Owner.GetPiece(moves[^1].Latter, out Piece attackingPiece);
                if (((attackingPiece?.MoveSet & MoveSets.Rook) != 0) && attackingPiece.Square != move.Latter)
                    return true;
                else
                    return false;
            }
            else if(Math.Abs(x) == Math.Abs(y))
            {
                x /= Math.Abs(x);               
                y /= Math.Abs(y);
                int directionX = move.Latter.Number.letter - move.Former.Number.letter;
                int directionY = move.Latter.Number.digit - move.Former.Number.digit;
                if (Math.Abs(directionY) == Math.Abs(directionX))
                {
                    if (x == (directionX / Math.Abs(directionX)) && y == (directionY / Math.Abs(directionY)))
                        return false;
                }
                moves = Move.GenerateMovesInADirection(piece.Square, (x, y), Owner, excludeMove: "moves");
                if (moves.Length == 0)
                    return false;
                Owner.GetPiece(moves[^1].Latter, out Piece attackingPiece);
                if (((attackingPiece?.MoveSet & MoveSets.Bishop) != 0) && attackingPiece.Square != move.Latter)
                    return true;
                else
                    return false;
            }
            return false;
        } //refactor
        public bool WouldTheMoveSolveAllTheThreats(Move move)
        {
            if (threats.Count == 0)
                return true;

            foreach (var threat in threats)
            {
                if(!threat.Contains(move.Latter))
                    return false;
            }

            return true;
        }
        public void ClearThreats() => threats.Clear();
        public void FindAllThreats()
        {
            ClearThreats();
            List<(MoveSets set, Move[] moves)> groupedMoves = new List<(MoveSets, Move[])>();
            groupedMoves.AddRange(Move.GenerateEveryMove(Square, MoveSets.Diagonal, Owner));
            groupedMoves.AddRange(Move.GenerateEveryMove(Square, MoveSets.Rook, Owner));
            groupedMoves.AddRange(Move.GenerateEveryMove(Square, MoveSets.Pawn, Owner));
            groupedMoves.AddRange(Move.GenerateEveryMove(Square, MoveSets.Knight, Owner));
            foreach (var grouping in groupedMoves)
            {
                if (grouping.moves.Length == 0)
                    continue;
                if (Owner.GetPiece(grouping.moves[^1].Latter, out Piece piece))
                {
                    if (piece != null && (grouping.set & piece.MoveSet) != 0)
                    {
                        var squares = (from m in grouping.moves select m.Latter).ToArray();
                        threats.Add(squares);
                    }
                }
            }
            if (threats.Count > 0)
                OnCheck(EventArgs.Empty);
        } //refactor
        public bool CheckIfSquareIsThreatened(Square checkedSquare)
        {
            List<Square> squareThreats = new List<Square>();

            List<(MoveSets set, Move[] moves)> groupedMoves = new List<(MoveSets, Move[])>();
            groupedMoves.AddRange(Move.GenerateEveryMove(checkedSquare, MoveSets.Diagonal, Owner, piece: this));
            groupedMoves.AddRange(Move.GenerateEveryMove(checkedSquare, MoveSets.Rook, Owner, piece: this));
            groupedMoves.AddRange(Move.GenerateEveryMove(checkedSquare, MoveSets.Pawn, Owner, piece: this));
            groupedMoves.AddRange(Move.GenerateEveryMove(checkedSquare, MoveSets.Knight, Owner, piece: this));
            groupedMoves.AddRange(Move.GenerateEveryMove(checkedSquare, MoveSets.King, Owner, piece: this));
            foreach (var group in groupedMoves)
            {
                if (group.moves.Length == 0)
                    continue;
                if (Owner.GetPiece(group.moves[^1].Latter, out Piece piece) && piece != null)
                {
                    if (group.moves[^1].Description == 'x' && (group.set & piece.MoveSet) != 0)
                    {
                        foreach (var move in group.moves)
                        {
                            squareThreats.Add(move.Latter);
                        }
                    }
                }
            }
            return squareThreats.Count > 0;
        } //refactor
        public override void MovePiece(Move move)
        {
            OnPieceMoved(new PieceMovedEventArgs(this, move));
            Square = move.Latter;
            CastlingRights = CastlingRights.None;
        }
        public void OnCheck(EventArgs e)
        {
            if (IsRawPiece)
                return;

            Check?.Invoke(this, e);
        }
    }
}
