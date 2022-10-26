using Chess.Board;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Chess.Graphics.UI;

internal class PieceGraveyard : IDrawableProvider
{
    private readonly List<DrawableObject> _whitePieces;
    private readonly List<DrawableObject> _blackPieces;
    private readonly int _maxPiecesInRow;
    private readonly int _pieceWidth;
    private bool _isInverted;
    private bool _skipPromotionPawn;
    private const float _propotionalDistanceFromEdges = 0.05f;
    private readonly Stack<(DrawableObject drawable, Team team)> _piecesToAdd;
    private Vector2 _zero;
    public int Width { get; private set; }
    public int Height { get; private set; }
    
    public PieceGraveyard(int widthOfAvailableSpace, int maxPiecesInRow, int numberOfRows)
    {
        _whitePieces = new List<DrawableObject>();
        _blackPieces = new List<DrawableObject>();
        _piecesToAdd = new();
        _pieceWidth = widthOfAvailableSpace/maxPiecesInRow;
        _maxPiecesInRow = maxPiecesInRow;
        SetSize(numberOfRows);
        SubscribeToEvents();
    }
    private void SubscribeToEvents()
    {
        Chessboard.PieceRemovedFromTheBoard += OnPieceRemovedFromTheBoard;
        Chessboard.BoardInverted += OnBoardInverted;
        Pawn.Promotion += OnPromotion;
        Chess.TurnEnded += OnTurnEnded;
    }
    private void SetSize(int numberOfRows)
    {
        Width = (int)(_maxPiecesInRow * _pieceWidth * (1 + _propotionalDistanceFromEdges * 2));
        Height = (int)(_pieceWidth * numberOfRows * (1 + _propotionalDistanceFromEdges * 2));
        _zero = new Vector2(Width * _propotionalDistanceFromEdges, Height * _propotionalDistanceFromEdges);
    }
    private void OnPromotion(object sender, EventArgs e) => _skipPromotionPawn = true;
    private void OnBoardInverted(object sender, EventArgs e)
    {
        foreach(var piece in _whitePieces)
        {
            if(_isInverted)
                piece.MoveObject(new Vector2(0, -(Height / 2)));
            else
                piece.MoveObject(new Vector2(0, Height / 2));
        }
        foreach(var piece in _blackPieces)
        {
            if(_isInverted)
                piece.MoveObject(new Vector2(0, Height / 2));
            else
                piece.MoveObject(new Vector2(0, -(Height / 2)));
        }
        _isInverted = !_isInverted;
    }
    private void OnPieceRemovedFromTheBoard(object sender, PieceEventArgs e)
    {
        Model blueprint = PieceFactory.CreatePieceDrawable(e.Piece).Model;
        DrawableObject deadPiece;
        int posX = (int)_zero.X;
        int posY = (int)_zero.Y;

        if (e.Piece.Team == Team.White)
        {
            posX += (_whitePieces.Count % _maxPiecesInRow) * _pieceWidth;
            posY += (_whitePieces.Count / _maxPiecesInRow) * _pieceWidth;
            if (_isInverted)
                posY += Height / 2;
            deadPiece = new DrawableObject(blueprint, new Rectangle(posX, posY, _pieceWidth, _pieceWidth));
        }
        else
        {
            posX += (_blackPieces.Count % _maxPiecesInRow) * _pieceWidth;
            posY += Height / 2 + (_blackPieces.Count / _maxPiecesInRow) * _pieceWidth;
            if (_isInverted)
                posY -= Height / 2;
            deadPiece = new DrawableObject(blueprint, new Rectangle(posX, posY, _pieceWidth, _pieceWidth));
        }
        _piecesToAdd.Push((deadPiece, e.Piece.Team));
    }
    private void OnTurnEnded(object sender, EventArgs e)
    {
        while(_piecesToAdd.TryPop(out (DrawableObject drawable, Team team) piece))
        {
            if (_skipPromotionPawn)
            {
                _skipPromotionPawn = false;
                continue;
            }

            if (piece.team == Team.White)
                _whitePieces.Add(piece.drawable);
            else
                _blackPieces.Add(piece.drawable);
        }
    }
    public IEnumerable<DrawableObject> GetDrawableObjects()
    {
        List<DrawableObject> drawables = new(_whitePieces);
        drawables.AddRange(_blackPieces);
        return drawables.ToArray();
    }
}
