using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using IDrawable = Chess.Graphics.IDrawable;

namespace Chess.Board;

internal sealed class Chessboard : IDrawable
{
    private static readonly int s_numberOfSquares = 8;
    private static Chessboard s_instance;
    public static Chessboard Instance { get 
        {
            if (s_instance == null)
                throw new InvalidOperationException("Chessboard instance not created!");
            else
                return s_instance;
        }}
    public static int NumberOfSquares { get => s_numberOfSquares; }
    public static event EventHandler<PieceEventArgs> PieceRemovedFromTheBoard;
    public static event EventHandler<PieceEventArgs> PieceAddedToTheBoard;
    public static event EventHandler BoardInverted;
    private Rectangle _textureRect;
    public Piece[] Pieces { get; init; }   
    public int SquareSideLength { get; init; }
    public bool Inverted { get; private set; }
    public Texture2D RawTexture { get; set; }
    public Rectangle TextureRect { get => _textureRect; set { _textureRect = value; } }
    public Rectangle DestinationRect { get; set; }
    public Color Color { get; set; }

    private Chessboard(Texture2D rawTexture, bool inverted = false)
    {
        s_instance = this;
        Pieces = new Piece[s_numberOfSquares * s_numberOfSquares];
        Inverted = inverted;
        RawTexture = rawTexture;
        TextureRect = new Rectangle(0, 0, rawTexture.Width / 2, rawTexture.Height);
        DestinationRect = new Rectangle(0, 0, rawTexture.Width / 2, rawTexture.Height);
        SquareSideLength = TextureRect.Width / s_numberOfSquares;
        Color = Color.White;
        Chess.PromotionConcluded += OnPromotionConcluded;
    }
    public static Chessboard Create(Texture2D rawTexture, bool inverted = false)
    {
        if (s_instance != null)
            throw new InvalidOperationException($"{nameof(Chessboard)} is already created!");
        return new Chessboard(rawTexture, inverted);
    }
    public void InitilizeBoard(Piece[] pieces)
    {
        foreach (Piece piece in pieces)
        {
            Pieces[piece.Square.Index] = piece;
        }
    }
    public void AddAPiece(Piece piece)
    {
        Pieces[piece.Square.Index] = piece;
        OnPieceAddedToTheBoard(new PieceEventArgs(Pieces[piece.Square.Index]));
    }
    public Piece CheckCollisions(int x, int y)
    {
        Square square = FromCords(x, y);
        if (!Square.Validate(square))
            return null;
        return Pieces[square.Index];
    }
    public void ToggleInversion()
    {
        int xOffset = RawTexture.Width / 2;
        if (Inverted)
            xOffset = -xOffset;

        Inverted = !Inverted;
        _textureRect.X += xOffset;
        OnBoardInverted(EventArgs.Empty);
    }
    public bool MovePiece(Piece targetedPiece, Square newSquare, out Move move)
    {
        Move? moveFromTargetedPiece = targetedPiece.GetAMove(in newSquare);
        if (moveFromTargetedPiece.HasValue)
        {
            move = moveFromTargetedPiece.Value;
            if (move.Description == MoveType.Takes)
                RemovePiece(move.Latter);

            Pieces[move.Former.Index] = null;
            Pieces[move.Latter.Index] = targetedPiece;
            if (move.Description == MoveType.EnPassant)
                RemovePiece(new Square(move.Latter.Letter, move.Former.Digit));
            else if (move.Description == MoveType.CastlesKingside || move.Description == MoveType.CastlesQueenside)
            {
                int direction = move.Former.Letter > move.Latter.Letter ? 1 : -1;
                Square originalRookPosition = King.GetCastlingRookSquare(move.Description, targetedPiece.Team);
                Square newRookPosition = new(move.Latter, (direction,0));
                Move rookMove = new(Pieces[originalRookPosition.Index].Square, newRookPosition, MoveType.ParticipatesInCastling);
                Pieces[originalRookPosition.Index].MovePiece(rookMove);
                Pieces[newRookPosition.Index] = Pieces[originalRookPosition.Index];
                Pieces[originalRookPosition.Index] = null;
            }
            targetedPiece.MovePiece(move);
            return true;
        }
        move = default;
        return false;
    }
    public void RemovePiece(Square square)
    {
        Piece piece = GetPiece(square);
        if (piece != null)
        {
            OnPieceRemovedFromTheBoard(new PieceEventArgs(piece));
            Pieces[square.Index] = null;
        }
    }
    public Piece GetPiece(in Square square) => Pieces[square.Index];
    public Team GetTeamOnSquare(in Square square) => Pieces[square.Index] == null ? Team.Empty : Pieces[square.Index].Team;
    public static (int, int) ConvertSquareToIndexes(Square square)
    {
        int x = square.Letter - 'a';
        int y = square.Digit - 1;
        return (y, x);
    }
    public Square FromVector(Vector2 vector) => FromCords((int)vector.X, (int)vector.Y);
    public Square FromCords(int x, int y)
    {
        int indexX = x < 0 ? -1 : x / SquareSideLength;
        int indexY = y < 0 ? -1 : Chessboard.s_numberOfSquares - 1 - y / SquareSideLength;

        if (Inverted)
        {
            double middle = (s_numberOfSquares - 1) / 2.0;
            indexY = (int)(middle + (middle - indexY));
            indexX = (int)(middle + (middle - indexX));
        }

        char letter = (char)('a' + indexX);
        int number = indexY + 1;

        return new Square(letter, number);
    }
    public Vector2 ToCordsFromSquare(Square square)
    {
        (int i, int j) = ConvertSquareToIndexes(square);
        if (Inverted)
        {
            double middle = (s_numberOfSquares - 1) / 2.0;
            j = (int)(middle + (middle - j));
            i = (int)(middle + (middle - i));
        }
        return new Vector2(j * SquareSideLength, (Chessboard.NumberOfSquares - i - 1) * SquareSideLength);
    }
    private void OnPromotionConcluded(object sender, PromotionEventArgs e)
    {
        Square square = e.PromotedPawn.Square;
        Team team = e.PromotedPawn.Team;
        RemovePiece(square);
        try
        {
            Piece newPiece = PieceFactory.CreateAPiece(e.TypeToBePromotedInto, square, team);
            newPiece.Owner = e.PromotedPawn.Owner;
            AddAPiece(newPiece);
        }
        catch (NotImplementedException)
        {
            throw;
        }
    }
    private void OnPieceRemovedFromTheBoard(PieceEventArgs e)
    {
        PieceRemovedFromTheBoard?.Invoke(this, e);
    }
    private void OnPieceAddedToTheBoard(PieceEventArgs e)
    {
        PieceAddedToTheBoard?.Invoke(this, e);
    }
    private void OnBoardInverted(EventArgs e) => BoardInverted?.Invoke(this, e);
}
