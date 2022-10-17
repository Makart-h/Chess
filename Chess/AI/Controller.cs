using Chess.Board;
using Chess.Graphics;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Chess.AI;

internal abstract class Controller : IPieceOwner, IDrawableProvider
{
    protected readonly List<Piece> _pieces;
    public King King { get; private set; }
    public Team Team { get; init; }
    public Piece[] Pieces { get { return _pieces.ToArray(); } }
    protected Dictionary<Piece, DrawableObject> _drawablePieces;
    public static event EventHandler<MoveMadeEventArgs> MoveMade;

    public Controller(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant)
    {
        Team = team;
        _pieces = new();
        _drawablePieces = new();
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
            _drawablePieces.Add(piece, PieceFactory.CreatePieceDrawable(piece));
        }
    }
    private void SubscribeToEvents()
    {
        Chessboard.PieceRemovedFromTheBoard += OnPieceRemovedFromTheBoard;
        Chessboard.PieceAddedToTheBoard += OnPieceAddedToTheBoard;
        Chessboard.BoardInverted += OnBoardInverted;
        Piece.PieceSelected += OnPieceSelected;
        Piece.PieceDeselected += OnPieceDeselected;
    }
    public King GetKing(Team team) => King;
    public DrawableObject GetDrawablePiece(Piece piece)
    {
        if (_drawablePieces.ContainsKey(piece))
            return _drawablePieces[piece];
        else
            return null;
    }
    public abstract DrawableObject[] GetDrawableObjects();
    public virtual void Update()
    {
        for (int i = _pieces.Count - 1; i >= 0; --i)
        {
            _pieces[i].Update();
        }
    }
    public abstract void MakeMove();
    protected virtual void OnMoveMade(MoveMadeEventArgs e)
    {
        MoveMade?.Invoke(this, e);
    }
    protected void OnPieceRemovedFromTheBoard(object sender, PieceEventArgs e)
    {
        _pieces.Remove(e.Piece);
        _drawablePieces.Remove(e.Piece);
    }
    protected void OnPieceAddedToTheBoard(object sender, PieceEventArgs e)
    {
        if (e.Piece.Team == Team)
        {
            _pieces.Add(e.Piece);
            _drawablePieces.Add(e.Piece, PieceFactory.CreatePieceDrawable(e.Piece));
        }
    }
    protected void OnPieceSelected(object sender, PieceEventArgs e)
    {
        if (e.Piece.Team == Team)
        {
            DrawableObject drawable = _drawablePieces[e.Piece];
            drawable.Color = new Color(drawable.Color, 10);
        }
    }
    protected void OnPieceDeselected(object sender, PieceEventArgs e)
    {
        if(e.Piece.Team == Team)
        {
            DrawableObject drawable = _drawablePieces[e.Piece];
            drawable.Color = new Color(drawable.Color, 255);
        }
    }
    protected void OnBoardInverted(object sender, EventArgs e)
    {
        foreach(DrawableObject drawablePiece in _drawablePieces.Values)
        {
            drawablePiece.RecalculatePosition();
        }
    }
    public Piece GetPiece(Square square) => Chessboard.Instance.GetPiece(square);
    public Team GetTeamOnSquare(Square square) => Chessboard.Instance.GetTeamOnSquare(square);
    public void OnPromotion(Piece piece) { }
}
