using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Board;

namespace Chess.Pieces
{
    class King : Piece
    {
        public CastlingRights CastlingRights { get; set; }
        private readonly List<List<Square>> threats;
        public static event EventHandler Check;
        public King(Team team, Square square, Texture2D rawTexture) : base(team, square)
        {
            model = new Graphics.Model(rawTexture, Square.SquareWidth * (int)PieceType.King, Square.SquareHeight * (int)team, Square.SquareWidth, Square.SquareHeight);
            moves = new List<Move>();
            threats = new List<List<Square>>();
            moveSet = MoveSets.King;
            CastlingRights = CastlingRights.None;
            Value = 0;
        }
        public King(King other) : base(other.team, other.square)
        {
            model = other.model;
            moves = new List<Move>(other.moves);
            CastlingRights = other.CastlingRights;
            Value = other.Value;
            moveSet = other.MoveSet;
        }
        public override void Update()
        {
            moves.Clear();
            threats.Clear();
            FindAllThreats();
            CheckPossibleMoves();
        }
        public override void CheckPossibleMoves()
        {
            CheckRegularMoves();
            
            if (threats.Count == 0)
            {
                if((CastlingRights & CastlingRights.KingSide) == CastlingRights.KingSide)
                    CheckCastling(GetKingSideCastleSquares(), Chessboard.Instance.GetAPiece(new Square((char)(square.Number.letter + 3), square.Number.digit)));
                if((CastlingRights & CastlingRights.QueenSide) == CastlingRights.QueenSide)
                    CheckCastling(GetQueenSideCastleSquares(), Chessboard.Instance.GetAPiece(new Square((char)(square.Number.letter - 4), square.Number.digit)));
            }
        }
        private void CheckRegularMoves()
        {
            foreach (var square in GetSquaresAroundTheKing())
            {
                Team teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);

                if (teamOnTheSquare == Team.Void || CheckIfSquareIsThreatened(square))
                    continue;

                if (teamOnTheSquare == Team.Empty)
                {
                    moves.Add(new Move(this.square, square, "moves"));
                }
                else if (teamOnTheSquare != team)
                {
                    moves.Add(new Move(this.square, square, "takes"));
                }
            }
        }
        private void CheckCastling(Square[] squaresToCheck, Piece potentialRook)
        {
            if (!(potentialRook is Rook r) || r.HasMoved)
                return;

            foreach(var square in squaresToCheck)
            {
                Team teamOnTheSquare = Chessboard.Instance.IsSquareOccupied(square);
                if (teamOnTheSquare != Team.Empty || CheckIfSquareIsThreatened(square))
                    return;
            }

            moves.Add(new Move(square, squaresToCheck[1], "castle:" + r.Square.ToString()));
        }
        public Square[] GetSquaresAroundTheKing()
        {
            return new Square[] {
                new Square((char)(square.Number.letter + 1), square.Number.digit + 1),
                new Square((char)(square.Number.letter + 1), square.Number.digit - 1),
                new Square((char)(square.Number.letter - 1), square.Number.digit + 1),
                new Square((char)(square.Number.letter - 1), square.Number.digit - 1),
                new Square((char)(square.Number.letter + 1), square.Number.digit),
                new Square((char)(square.Number.letter - 1), square.Number.digit),
                new Square(square.Number.letter, square.Number.digit + 1),
                new Square(square.Number.letter, square.Number.digit - 1),
            };
        }
        private Square[] GetKingSideCastleSquares()
        {
            return new Square[]
            {
                new Square((char)(square.Number.letter + 1), square.Number.digit),
                new Square((char)(square.Number.letter + 2), square.Number.digit)
            };
        }
        private Square[] GetQueenSideCastleSquares()
        {
            return new Square[]
            {
                new Square((char)(square.Number.letter - 1), square.Number.digit),
                new Square((char)(square.Number.letter - 2), square.Number.digit),
                new Square((char)(square.Number.letter - 3), square.Number.digit)
            };
        }
        public bool CheckMoveAgainstThreats(Piece piece, Move move) => !WouldTheMoveGenerateAThreat(piece, move) && WouldTheMoveSolveAllTheThreats(move);
        public bool WouldTheMoveGenerateAThreat(Piece piece, Move move)
        {
            List<Move> moves;
            if (!Chessboard.Instance.ArePiecesFacingEachOther(this, piece))
                return false;

            int x = square.Number.letter - move.Former.Number.letter;
            int y = square.Number.digit - move.Former.Number.digit;
            if(x == 0)
            {
                y /= Math.Abs(y);
                y = -y;
                moves = Move.GenerateMovesInADirection(piece, (s => new Square(s.Number.letter, s.Number.digit + y)), excludeMove:"moves");
                if (moves.Count == 0)
                    return false;
                Piece attackingPiece = Chessboard.Instance.GetAPiece(moves[^1].Latter);
                if (((attackingPiece?.MoveSet & MoveSets.Rook) != 0) && attackingPiece.Square != move.Latter)
                    return true;
                else
                    return false;
            }
            else if(y == 0)
            {
                x /= Math.Abs(x);
                x = -x;
                moves = Move.GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter + x), s.Number.digit)), excludeMove: "moves");
                if (moves.Count == 0)
                    return false;
                Piece attackingPiece = Chessboard.Instance.GetAPiece(moves[^1].Latter);
                if (((attackingPiece?.MoveSet & MoveSets.Rook) != 0) && attackingPiece.Square != move.Latter)
                    return true;
                else
                    return false;
            }
            else if(Math.Abs(x) == Math.Abs(y))
            {
                x /= Math.Abs(x);
                x = -x;
                y /= Math.Abs(y);
                y = -y;
                moves = Move.GenerateMovesInADirection(piece, (s => new Square((char)(s.Number.letter+x), s.Number.digit + y)), excludeMove: "moves");
                if (moves.Count == 0)
                    return false;
                Piece attackingPiece = Chessboard.Instance.GetAPiece(moves[^1].Latter);
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
            else
            {
                foreach (var item in threats)
                {
                    if (!item.Contains(move.Latter))
                        return false;
                }
            }
            return true;
        } //refactor
        public void ClearThreats() => threats.Clear();
        public void FindAllThreats()
        {
            List<(MoveSets set, List<Move> moves)> groupedMoves = new List<(MoveSets, List<Move>)>();
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Diagonal));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Rook));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Pawn));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Knight));
            foreach (var item in groupedMoves)
            {
                if (item.moves.Count == 0)
                    continue;
                Piece piece = Chessboard.Instance.GetAPiece(item.moves[^1].Latter);
                if (piece != null && (item.set & piece.MoveSet) != 0)
                {
                    List<Square> squares = new List<Square>();
                    foreach (var move in item.moves)
                    {
                        squares.Add(move.Latter);
                    }
                    threats.Add(squares);
                }
            }
            if (threats.Count > 0)
                OnCheck(EventArgs.Empty);
        } //refactor
        public bool CheckIfSquareIsThreatened(Square checkedSquare)
        {
            Square kingSquare = this.Square;
            this.Square = checkedSquare;
            List<Square> squareThreats = new List<Square>();

            List<(MoveSets set, List<Move> moves)> groupedMoves = new List<(MoveSets, List<Move>)>();
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Diagonal));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Rook));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Pawn));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.Knight));
            groupedMoves.AddRange(Move.GenerateEveryMove(this, MoveSets.King));
            foreach (var item in groupedMoves)
            {
                if (item.moves.Count == 0)
                    continue;
                Piece piece = Chessboard.Instance.GetAPiece(item.moves[^1].Latter);
                if (piece != null && (item.set & piece.MoveSet) != 0)
                {
                    List<Square> squares = new List<Square>();
                    foreach (var move in item.moves)
                    {
                        squares.Add(move.Latter);
                    }
                    squareThreats.AddRange(squares);
                }
            }

            this.Square = kingSquare;
            return squareThreats.Count > 0;
        } //refactor
        public override void MovePiece(Move move)
        {
            OnPieceMoved(new PieceMovedEventArgs(this, move));
            this.square = move.Latter;
            CastlingRights = CastlingRights.None;
        }

        public void OnCheck(EventArgs e)
        {
            Check?.Invoke(this, e);
        }
    }
}
