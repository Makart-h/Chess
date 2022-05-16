using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Graphics;
using Chess.Pieces;
using Chess.Data;
using System.Linq;

namespace Chess.Board
{
    class Chessboard : DrawableObject
    {
        public static Chessboard Instance;
        private readonly Dictionary<Square, Piece> pieces;
        static readonly int numberOfSquares = 8;
        public static int NumberOfSquares {get => numberOfSquares;}
        public static event EventHandler<PieceRemovedFromTheBoardEventArgs> PieceRemovedFromTheBoard;

        public Chessboard(Texture2D rawTexture)
        {
            model = new Graphics.Model(rawTexture, 0, 0, numberOfSquares * Square.SquareWidth, numberOfSquares * Square.SquareHeight);
            position = new Vector2(0, 0);
            Instance = this;
            pieces = new Dictionary<Square, Piece>();
            foreach(var letter in Enumerable.Range('a', NumberOfSquares).Select(n => (char)n))
            {
                foreach (var number in from n in Enumerable.Range(1, numberOfSquares)
                                       select int.Parse(n.ToString()))
                {
                    pieces[new Square(letter, number)] = null;
                }
            }
        }
        public void InitilizeBoard(Piece[] pieces)
        {
            foreach (var piece in pieces)
            {
                this.pieces[piece.Square] = piece;
            }
        }
        public void AddAPiece(Piece piece)
        {
            pieces[piece.Square] = piece;
        }
        public Piece CheckCollisions(int x, int y)
        {
            Square square = FromCords(x, y);

            return pieces[square];
        }
        public bool MovePiece(Piece targetedPiece, Square newSquare, out Move move)
        {
            List<Move> moves = targetedPiece.Moves;
            move = targetedPiece.GetAMove(newSquare);
            if (move != null)
            {
                if (move.Description == "takes")
                {
                    RemoveAPiece(move.Latter);
                }
                pieces[move.Former] = null;
                pieces[move.Latter] = targetedPiece;
                moves.Clear();
                if (move.Description == "en passant")
                    RemoveAPiece(new Square(move.Latter.Number.letter, move.Former.Number.digit));
                else if(move.Description.Contains("castle"))
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
                targetedPiece.MovePiece(move);
                return true;
            }
            return false;
        }
        public void RemoveAPiece(Square square)
        {
            if(pieces[square] != null)
            {         
                OnPieceRemovedFromTheBoard(new PieceRemovedFromTheBoardEventArgs(pieces[square]));
                pieces[square] = null;
            }
        }
        public Piece GetAPiece(Square square) => pieces.TryGetValue(square, out Piece piece) ? piece : null;
        public Team IsSquareOccupied(Square square)
        {
            (int y, int x) = ConvertSquareToIndexes(square);
            if (x < 0 || y < 0 || x >= numberOfSquares || y >= numberOfSquares)
                return Team.Void;
            return pieces[square] == null ? Team.Empty : pieces[square].Team;
        }
        public (int, int) ConvertSquareToIndexes(Square square)
        {
            int x = square.Number.letter - 'A';
            int y = square.Number.digit - 1;
            return (y, x);
        }
        public bool ArePiecesFacingEachOther(Piece first, Piece second)
        {
            if (first.Square.Number.letter == second.Square.Number.letter)
            {
                int direction = first.Square.Number.digit > second.Square.Number.digit ? -1 : 1;
                for (int i = first.Square.Number.digit + direction; ; i += direction)
                {
                    Piece pieceOnTheWay = GetAPiece(new Square(first.Square.Number.letter, i));
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
                    Piece pieceOnTheWay = GetAPiece(new Square((char)i, first.Square.Number.digit));
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
                    Piece pieceOnTheWay = GetAPiece(new Square((char)i, j));
                    if (pieceOnTheWay == null)
                        continue;

                    return pieceOnTheWay == second;
                }
            }
            return false;
        }
        public static Square FromCords(int x, int y)
        {
            int indexX = x / Square.SquareWidth;
            int indexY = Chessboard.numberOfSquares - 1 - y / Square.SquareHeight;

            char letter = (char)('A' + indexX);
            int number = indexY + 1;

            return new Square(letter, number);
        }
        public static Vector2 ToCordsFromSquare(Square square)
        {
            (int i, int j) = Chessboard.Instance.ConvertSquareToIndexes(square);
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
                Piece newPiece = PieceFactory.Instance.CreateAPiece(type, square, team);
                AddAPiece(newPiece);
            }
            catch(NotImplementedException)
            {
                //log the exception and probbably shutdown the game
            }
        }

        public PieceType GetPromotionType()
        {
            return PieceType.Queen;
        }

        public void OnPieceRemovedFromTheBoard(PieceRemovedFromTheBoardEventArgs args)
        {
            PieceRemovedFromTheBoard?.Invoke(this, args);
        }
    }
}
