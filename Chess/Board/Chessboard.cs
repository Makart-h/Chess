using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Chess.Board
{
    internal sealed class Chessboard : DrawableObject
    {
        public static Chessboard Instance { get; private set; }
        private readonly Dictionary<Square, Piece> _pieces;
        public Dictionary<Square, Piece> Pieces { get { return _pieces; } }
        private static readonly int s_numberOfSquares = 8;
        public static int NumberOfSquares {get => s_numberOfSquares;}
        public static event EventHandler<PieceEventArgs> PieceRemovedFromTheBoard;
        public static event EventHandler<PieceEventArgs> PieceAddedToTheBoard;
        public static event EventHandler BoardInverted;
        public bool Inverted { get; private set; }

        private Chessboard(Texture2D rawTexture, bool inverted = false) : base(null, new Vector2())
        {
            Instance = this;
            Model = new Graphics.Model(rawTexture, 0, 0, s_numberOfSquares * Square.SquareWidth, s_numberOfSquares * Square.SquareHeight);
            _pieces = new Dictionary<Square, Piece>();
            foreach (var letter in Enumerable.Range('a', NumberOfSquares).Select(n => (char)n))
            {
                foreach (var number in from n in Enumerable.Range(1, s_numberOfSquares)
                                       select int.Parse(n.ToString()))
                {
                    _pieces[new Square(letter, number)] = null;
                }
            }
            Inverted = inverted;
        }
        public static Chessboard Create(Texture2D rawTexture, bool inverted = false)
        {
            if (Instance != null)
                throw new InvalidOperationException($"{nameof(Chessboard)} is already created!");
            return new Chessboard(rawTexture, inverted);
        }
        public void InitilizeBoard(Piece[] pieces)
        {
            foreach (Piece piece in pieces)
            {
                _pieces[piece.Square] = piece;
            }
        }
        public void AddAPiece(Piece piece)
        {
            _pieces[piece.Square] = piece;
            OnPieceAddedToTheBoard(new PieceEventArgs(_pieces[piece.Square]));
        }
        public Piece CheckCollisions(int x, int y)
        {
            Square square = FromCords(x, y);
            if (_pieces.TryGetValue(square, out Piece piece))
                return piece;
            else
                return null;
        }
        public void ToggleInversion()
        {
            Inverted = !Inverted;
            OnBoardInverted(EventArgs.Empty);
        }
        public bool MovePiece(Piece targetedPiece, Square newSquare, out Move move)
        {
            move = targetedPiece.GetAMove(newSquare);
            if (move != null)
            {
                if (move.Description == 'x')
                {
                    RemoveAPiece(move.Latter);
                }
                _pieces[move.Former] = null;
                _pieces[move.Latter] = targetedPiece;
                if (move.Description == 'p')
                    RemoveAPiece(new Square(move.Latter.Letter, move.Former.Digit));
                else if(move.Description == 'k' || move.Description == 'q')
                {
                    int direction = move.Former.Letter > move.Latter.Letter ? 1 : -1;
                    Square originalRookPosition = King.GetCastlingRookSquare(move.Description, targetedPiece.Team);
                    Square newRookPosition = new Square((char)(move.Latter.Letter + direction), move.Latter.Digit);
                    Move rookMove = new Move(_pieces[originalRookPosition].Square, newRookPosition, 'c');
                    _pieces[originalRookPosition].MovePiece(rookMove);
                    _pieces[newRookPosition] = _pieces[originalRookPosition];
                    _pieces[originalRookPosition] = null;                  
                }
                targetedPiece.MovePiece(move);
                return true;
            }
            return false;
        }
        public void RemoveAPiece(Square square)
        {
            if(_pieces[square] != null)
            {         
                OnPieceRemovedFromTheBoard(new PieceEventArgs(_pieces[square]));
                _pieces[square] = null;
            }
        }
        public bool GetAPiece(Square square, out Piece piece) => Pieces.TryGetValue(square, out piece);
        public Team IsSquareOccupied(Square square)
        {
            (int y, int x) = ConvertSquareToIndexes(square);
            if (x < 0 || y < 0 || x >= s_numberOfSquares || y >= s_numberOfSquares)
                return Team.Void;
            return _pieces[square] == null ? Team.Empty : _pieces[square].Team;
        }
        public static (int, int) ConvertSquareToIndexes(Square square)
        {
            int x = square.Letter - 'A';
            int y = square.Digit - 1;
            return (y, x);
        }
        public bool ArePiecesFacingEachOther(Piece first, Piece second)
        {
            if (first.Square.Letter == second.Square.Letter)
            {
                int direction = first.Square.Digit > second.Square.Digit ? -1 : 1;
                for (int i = first.Square.Digit + direction; ; i += direction)
                {
                    if (!GetAPiece(new Square(first.Square.Letter, i), out Piece pieceOnTheWay))
                        return false;
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;
                }
            }
            else if (first.Square.Digit == second.Square.Digit)
            {
                int direction = first.Square.Letter > second.Square.Letter ? -1 : 1;
                for (int i = first.Square.Letter + direction; ; i += direction)
                {
                    if (!GetAPiece(new Square((char)i, first.Square.Digit), out Piece pieceOnTheWay))
                        return false;
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;
                }
            }
            else if (Math.Abs(first.Square.Digit - second.Square.Digit) == Math.Abs(first.Square.Letter - second.Square.Letter))
            {
                int directionLetter = first.Square.Letter > second.Square.Letter ? -1 : 1;
                int directionDigit = first.Square.Digit > second.Square.Digit ? -1 : 1;
                for (int i = first.Square.Letter + directionLetter, j = first.Square.Digit + directionDigit; ; i += directionLetter, j += directionDigit)
                {

                    if (!GetAPiece(new Square((char)i, j), out Piece pieceOnTheWay))
                        return false;
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;
                }
            }
            return false;
        }
        public Square FromVector(Vector2 vector) => FromCords((int)vector.X, (int)vector.Y);
        public Square FromCords(int x, int y)
        {
            int indexX = x < 0 ? -1 : x / Square.SquareWidth;
            int indexY = y < 0 ? -1 : Chessboard.s_numberOfSquares - 1 - y / Square.SquareHeight;

            if (Inverted)
            {
                double middle = (s_numberOfSquares - 1) / 2.0;
                indexY = (int)(middle + (middle - indexY));
                indexX = (int)(middle + (middle - indexX));
            }

            char letter = (char)('A' + indexX);
            int number = indexY + 1;

            return new Square(letter, number);
        }
        public Vector2 ToCordsFromSquare(Square square)
        {
            (int i, int j) = ConvertSquareToIndexes(square);
            if(Inverted)
            {
                double middle = (s_numberOfSquares-1) / 2.0;
                j = (int)(middle + (middle - j));
                i = (int)(middle + (middle - i));
            }
            return new Vector2(j * Square.SquareWidth, (Chessboard.NumberOfSquares - i - 1) * Square.SquareHeight);
        }
        public void OnPromotion(Piece piece)
        {
            PieceType type = GetPromotionType();
            Square square = piece.Square;
            Team team = piece.Team;
            RemoveAPiece(square);
            try
            {
                Piece newPiece = PieceFactory.CreateAPiece(type, square, team);
                newPiece.Owner = piece.Owner;
                AddAPiece(newPiece);               
            }
            catch(NotImplementedException)
            {
                throw;
            }
        }
        public PieceType GetPromotionType()
        {
            return PieceType.Queen;
        }
        public void OnPieceRemovedFromTheBoard(PieceEventArgs args)
        {
            PieceRemovedFromTheBoard?.Invoke(this, args);
        }
        public void OnPieceAddedToTheBoard(PieceEventArgs args)
        {
            PieceAddedToTheBoard?.Invoke(this, args);
        }
        public void OnBoardInverted(EventArgs args)
        {
            foreach (Piece piece in _pieces.Values)
                piece?.RecalculatePosition();

            BoardInverted?.Invoke(this, args);
        }
    }
}
