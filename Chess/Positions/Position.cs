using System;
using System.Collections.Generic;
using Chess.Pieces;
using Chess.Movement;
using Chess.Board;
using System.Linq;
using System.Threading.Tasks;
using Chess.Data;
using System.Threading;

namespace Chess.Positions
{
    internal class Position : IPieceOwner
    {
        private readonly int halfMoves;
        private King white;
        private King black;
        public string ShortFEN { get; private set; } 
        public King White { get => white; }
        public King Black { get => black; }
        public Team ActiveTeam { get; private set; }
        public int HalfMoves { get => halfMoves; }
        public bool Check { get; private set; }
        public Dictionary<Square, Piece> Pieces { get; private set; }
        public List<Move> NextMoves{ get; private set; }
        private Position(Dictionary<Square, Piece> pieces, Team activeTeam, Move move, int halfMoves)
        {
            Pieces = new Dictionary<Square, Piece>(Chessboard.NumberOfSquares*Chessboard.NumberOfSquares);
            NextMoves = new List<Move>();
            ActiveTeam = activeTeam;
            this.halfMoves = halfMoves;
            Check = false;
            CopyDictionary(pieces);
            ApplyMove(move);
            Update();
        }
        private Position(Position other, Move move) : this(other.Pieces, other.ActiveTeam, move, other.halfMoves) { }
        private Position(Chessboard board, Team activeTeam, Move move, int halfMoves = 0) : this(board.Pieces, activeTeam, move, halfMoves) { }
        public static Task<Position> CreateAsync(Chessboard board, Team activeTeam, Move move, CancellationToken token, int halfMoves = 0)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                Position position = new Position(board, activeTeam, move, halfMoves);
                return position;
            });
        }
        public static Task<Position> CreateAsync(CancellationToken token, Position other, Move move)
        {
            return Task.Run(() =>
            {
                token.ThrowIfCancellationRequested();
                Position position = new Position(other, move);
                return position;
            });
        }
        private void CopyDictionary(Dictionary<Square, Piece> other)
        {
            Piece copiedPiece;
            foreach (var key in other.Keys)
            {
                copiedPiece = other[key];
                if (copiedPiece != null)
                {
                    Piece createdPiece = PieceFactory.CopyAPiece(copiedPiece, this, true);
                    Pieces[key] = createdPiece;
                    if (createdPiece is King k)
                    {
                        if (k.Team == Team.White)
                            white = k;
                        else
                            black = k;
                    }
                }
                else
                {
                    Pieces[key] = null;
                }
            }
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
            move = Pieces[move.Former]?.GetAMove(move.Latter);
            if (move != null)
            {
                Pieces[move.Latter] = Pieces[move.Former];
                Pieces[move.Former] = null;
                if (move.Description == 'p')
                {
                    Square enPassant = new Square(move.Latter.Number.letter, move.Former.Number.digit);
                    Pieces[enPassant] = null;
                }
                else if (move.Description == 'k' || move.Description == 'q')
                {
                    int direction = move.Former.Number.letter > move.Latter.Number.letter ? 1 : -1;
                    Square originalRookPosition = King.GetCastlingRookSquare(move.Description, Pieces[move.Latter].Team);
                    Square newRookPosition = new Square((char)(move.Latter.Number.letter + direction), move.Latter.Number.digit);
                    Move rookMove = new Move(Pieces[originalRookPosition].Square, newRookPosition, 'c');
                    Pieces[originalRookPosition].MovePiece(rookMove);
                    Pieces[newRookPosition] = Pieces[originalRookPosition];
                    Pieces[originalRookPosition] = null;

                }
                Pieces[move.Latter].MovePiece(move);
            }
            else
                throw new InvalidOperationException("No such move on the given piece!");
        }
        private void Update()
        {
            ActiveTeam = ~ActiveTeam;
            if (white.Team == ActiveTeam)
            {
                white.Update();
                black.CheckCastlingMoves();
                if (white.Moves.Count > 0)
                {
                    NextMoves.AddRange(white.Moves);
                }
                if (white.Threatened)
                    Check = true;
            }
            else if (black.Team == ActiveTeam)
            {
                black.Update();
                white.CheckCastlingMoves();
                if (black.Moves.Count > 0)
                {
                    NextMoves.AddRange(black.Moves);
                }
                if (black.Threatened)
                    Check = true;
            }
            foreach (var piece in Pieces.Values)
            {
                if (piece is King)
                    continue;
                else if(piece != null && piece.Team == ActiveTeam)
                {
                    piece.Update();
                    if(piece.Moves.Count > 0)
                        NextMoves.AddRange(piece.Moves);
                }
            }       
            ShortFEN = FENParser.ToShortFenString(Pieces, white.CastlingRights, black.CastlingRights, ActiveTeam);
        }
        public void PrepareForEvaluation()
        {
            if(ActiveTeam == Team.White)
            {
                black.FindAllThreats();
            }
            else
            {
                white.FindAllThreats();
            }
        }
        public bool GetPiece(Square square, out Piece piece) => Pieces.TryGetValue(square, out piece);
        public Team IsSquareOccupied(Square square)
        {
            (int y, int x) = Chessboard.ConvertSquareToIndexes(square);
            if (x < 0 || y < 0 || x >= Chessboard.NumberOfSquares || y >= Chessboard.NumberOfSquares)
                return Team.Void;
            return Pieces[square] == null ? Team.Empty : Pieces[square].Team;
        }
        public bool ArePiecesFacingEachOther(Piece first, Piece second)
        {
            if (first.Square.Number.letter == second.Square.Number.letter)
            {
                int direction = first.Square.Number.digit > second.Square.Number.digit ? -1 : 1;
                for (int i = first.Square.Number.digit + direction; ; i += direction)
                {
                    if (!GetPiece(new Square(first.Square.Number.letter, i), out Piece pieceOnTheWay))
                        return false;
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;
                }
            }
            else if (first.Square.Number.digit == second.Square.Number.digit)
            {
                int direction = first.Square.Number.letter > second.Square.Number.letter ? -1 : 1;
                for (int i = first.Square.Number.letter + direction; ; i += direction)
                {
                    if (!GetPiece(new Square((char)i, first.Square.Number.digit), out Piece pieceOnTheWay))
                        return false;
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;
                }
            }
            else if (Math.Abs(first.Square.Number.digit - second.Square.Number.digit) == Math.Abs(first.Square.Number.letter - second.Square.Number.letter))
            {
                int directionLetter = first.Square.Number.letter > second.Square.Number.letter ? -1 : 1;
                int directionDigit = first.Square.Number.digit > second.Square.Number.digit ? -1 : 1;
                for (int i = first.Square.Number.letter + directionLetter, j = first.Square.Number.digit + directionDigit; ; i += directionLetter, j += directionDigit)
                {

                    if (!GetPiece(new Square((char)i, j), out Piece pieceOnTheWay))
                        return false;
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;

                }
            }
            return false;
        }
        public void OnPromotion(Piece piece)
        {
            PieceType type = PieceType.Queen;
            Square square = piece.Square;
            Team team = piece.Team;
            Pieces[square] = null;
            try
            {
                Piece newPiece = PieceFactory.CreateAPiece(type, square, team);
                newPiece.Owner = piece.Owner;
                Pieces[square] = newPiece;
            }
            catch (NotImplementedException)
            {
                //log the exception and probbably shutdown the game
            }
        }
    }
}
