using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Chess.Movement;
using Chess.Graphics;
using Chess.Pieces;
using System.Linq;

namespace Chess.Board
{
    class Chessboard : DrawableObject
    {
        public static Chessboard Instance;
        private readonly (Square square, Piece piece)[,] squares;
        private readonly List<DrawableObject> pieces;
        public List<DrawableObject> Pieces { get => pieces; }
        public (Square, Piece)[,] Squares { get => squares; }
        static readonly int numberOfSquares = 8;
        public static int NumberOfSquares {get => numberOfSquares;}
        private King whiteKing;
        private King blackKing;
        private Team toMove;
        public Team ToMove { get => toMove; }

        public Chessboard(Texture2D rawTexture)
        {
            squares = new (Square, Piece)[numberOfSquares, numberOfSquares];
            model = new Graphics.Model(rawTexture, 0, 0, numberOfSquares * Square.SquareWidth, numberOfSquares * Square.SquareHeight);
            position = new Vector2(0, 0);
            for (int i = 0; i < numberOfSquares; ++i)
            {
                for (int j = 0; j < numberOfSquares; ++j)
                {
                    squares[i, j].square = new Square((char)('A' + j), i + 1);
                    squares[i, j].piece = null;
                }
            }
            pieces = new List<DrawableObject>();
            Instance = this;
        }
        #region BoardInitialization
        public void InitilizeBoard(string FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            
            var fields = FEN.Split();
            if (fields.Length != 6)
                throw new ArgumentException();
            ParsePieces(fields[0]);
            ParseMoveOrder(fields[1]);
            ParseCastlingRights(fields[2]);
            try
            {
                ParseEnPassant(fields[3]);
            }
            catch(ArgumentException)
            {
                //log error
                throw;
            }
        }
        private void ParsePieces(string fenField)
        {
            int x = 8;
            int y = 'A';
            foreach (char c in fenField)
            {
                if (c == '/')
                {
                    x--;
                    y = 'A';
                }
                else if (char.IsDigit(c))
                {
                    y += c - '0';
                }
                else
                {
                    try
                    {
                        Piece piece = PieceFactory.Instance.CreateAPiece(c, new Square((char)y, x));
                        AddAPiece(piece);
                    }
                    catch (NotImplementedException)
                    {
                        //log the exception or perphaps stop initilizing the board since the string might be wrong
                    }
                    finally
                    {
                        y++;
                    }
                }
            }
        }
        private void ParseMoveOrder(string fenField) => toMove = fenField.Contains("w") ? Team.White : Team.Black;
        private void ParseCastlingRights(string fenField)
        {
            foreach (var c in fenField)
            {
                switch (c)
                {
                    case 'K':
                        whiteKing.CanCastleKingSide = true;
                        break;
                    case 'Q':
                        whiteKing.CanCastleQueenSide = true;
                        break;
                    case 'k':
                        blackKing.CanCastleKingSide = true;
                        break;
                    case 'q':
                        blackKing.CanCastleQueenSide = true;
                        break;
                }
            }
        }
        private void ParseEnPassant(string fenField)
        {
            if (!fenField.Contains('-'))
            {
                if (fenField.Length != 2)
                    throw new ArgumentException();

                int direction = toMove == Team.White ? -1 : 1;
                Square square = new Square(fenField[0], fenField[1] - '0' + direction);
                if (GetAPiece(square) is Pawn p)
                    p.EnPassant = true;
            }
        }
        #endregion
        public King GetKing(Team team) => team == Team.Black ? blackKing : whiteKing;
        public void AddAPiece(Piece piece)
        {
            (int y, int x) = ConvertSquareToIndexes(piece.Square);
            if(piece is King k)
            {
                if (k.Team == Team.Black)
                    blackKing = k;
                else
                    whiteKing = k;
            }
            squares[y, x].piece = piece;
            pieces.Add(piece);
        }
        public (Square, Piece) CheckCollisions(int x, int y)
        {
            int indexX = x / Square.SquareWidth;
            int indexY = numberOfSquares - 1 - y / Square.SquareHeight;

            return squares[indexY, indexX];
        }
        public (Square, Piece) IsAValidPieceToMove((Square s, Piece p) square)
        {
            if (square.p != null && square.p.Team == toMove)
                return square;

            return (square.s, null);
        }
        public bool MovePiece((Square Square, Piece Piece) targetedPiece, Square newSquare)
        {
            List<Move> moves = targetedPiece.Piece.Moves;
            Move move = targetedPiece.Piece.GetAMove(newSquare);
            if (move != null)
            {
                (int oldX, int oldY) = ConvertSquareToIndexes(move.Former);               
                (int x, int y) = ConvertSquareToIndexes(move.Latter);
                if (move.Description == "takes")
                {
                    RemoveAPiece(move.Latter);
                }                   
                squares[oldX, oldY].piece = null;
                squares[x, y].piece = targetedPiece.Piece;
                moves.Clear();
                toMove = toMove == Team.Black ? Team.White : Team.Black;
                if (move.Description == "en passant")
                    RemoveAPiece(new Square(move.Latter.Number.letter, move.Former.Number.digit));
                else if(move.Description.Contains("castle"))
                {
                    int direction = (int)move.Former.Number.letter > (int)move.Latter.Number.letter ? 1 : -1;
                    string square = move.Description.Split(':')[1];
                    (x, y) = ConvertSquareToIndexes(new Square(square[0], int.Parse(square[1].ToString())));
                    Square newRookPosition = new Square((char)(move.Latter.Number.letter + direction), move.Latter.Number.digit);
                    Move rookMove = new Move(squares[x, y].piece.Square, newRookPosition, "castle");
                    squares[x, y].piece.MovePiece(rookMove);
                    (int i, int j) = ConvertSquareToIndexes(newRookPosition);
                    squares[i, j].piece = squares[x, y].piece;
                    squares[x, y].piece = null;
                    
                }
                targetedPiece.Piece.MovePiece(move);
                return true;
            }
            return false;
        }
        public GameState GetNewGameState() => new GameState(this.squares, (toMove == Team.White ? Team.Black : Team.White));
        public void RemoveAPiece(Square square)
        {
            (int y, int x) = ConvertSquareToIndexes(square);
            if (x < 0 || x >= numberOfSquares || y < 0 || y > numberOfSquares)
                return;
            else
            {
                pieces.Remove(squares[y, x].piece);
                squares[y, x].piece = null;
            }
        }
        public Piece GetAPiece(Square square)
        {
            (int y, int x) = ConvertSquareToIndexes(square);
            if (x < 0 || x >= numberOfSquares || y < 0 || y > numberOfSquares)
                return null;
            else
                return squares[y, x].Item2;
        }
        public Team IsSquareOccupied(Square square)
        {
            (int y, int x) = ConvertSquareToIndexes(square);
            if (x < 0 || y < 0 || x >= numberOfSquares || y >= numberOfSquares)
                return Team.Void;
            return squares[y, x].piece == null ? Team.Empty : squares[y, x].piece.Team;
        }
        public bool IsSquareUnderAttack(Square square, Team team)
        {
            foreach (var item in squares)
            {
                if (item.Item2 == null)
                    continue;
                if (item.Item2.Team == team)
                    continue;
                else if (item.Item2.CanMoveToSquare(square))
                    return true;            
            }
            return false;
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

                    return pieceOnTheWay == second ? true : false;
                }
            }
            return false;
        }
        public void Update()
        {
            switch (toMove)
            {
                case Team.White:
                    whiteKing.ClearThreats();
                    whiteKing.FindAllThreats();
                    break;
                case Team.Black:
                    blackKing.ClearThreats();
                    blackKing.FindAllThreats();
                    break;
            }
            foreach(var item in squares)
            {
                if (item.piece == null || item.piece.Team != toMove)
                    continue;
                item.piece.Update();              
            }
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
    }
}
