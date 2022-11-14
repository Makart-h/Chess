using Chess.Board;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using IDrawable = Chess.Graphics.IDrawable;

namespace Chess.AI;

internal abstract class Controller : IPieceOwner, IDrawableProvider
{
    protected readonly List<Piece> _pieces;
    public King King { get; private set; }
    public Team Team { get; init; }
    public Piece[] Pieces { get { return _pieces.ToArray(); } }
    protected Dictionary<Piece, IMovableDrawable> _piecesModels;
    public static event EventHandler<MoveMadeEventArgs> MoveMade;

    public Controller(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant)
    {
        Team = team;
        _pieces = new();
        _piecesModels = new();
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
            _piecesModels.Add(piece, PieceFactory.CreatePieceModel(piece));
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
    public IMovableDrawable GetPieceModel(Piece piece)
    {
        if (_piecesModels.ContainsKey(piece))
            return _piecesModels[piece];
        else
            return null;
    }
    public abstract IEnumerable<IDrawable> GetDrawableObjects();
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
        _piecesModels.Remove(e.Piece);
    }
    protected void OnPieceAddedToTheBoard(object sender, PieceEventArgs e)
    {
        if (e.Piece.Team == Team)
        {
            _pieces.Add(e.Piece);
            _piecesModels.Add(e.Piece, PieceFactory.CreatePieceModel(e.Piece));
        }
    }
    protected void OnPieceSelected(object sender, PieceEventArgs e)
    {
        if (e.Piece.Team == Team)
        {
            IDrawable pieceModel = _piecesModels[e.Piece];
            pieceModel.Color = new Color(pieceModel.Color, 10);
        }
    }
    protected void OnPieceDeselected(object sender, PieceEventArgs e)
    {
        if(e.Piece.Team == Team)
        {
            IDrawable pieceModel = _piecesModels[e.Piece];
            pieceModel.Color = new Color(pieceModel.Color, 255);
        }
    }
    protected void OnBoardInverted(object sender, EventArgs e)
    {
        foreach(IMovable pieceModel in _piecesModels.Values)
        {
            pieceModel.Position = MovementManager.RecalculateVector(pieceModel.Position);
        }
    }
    public Piece GetPiece(in Square square) => Chessboard.Instance.GetPiece(in square);
    public Team GetTeamOnSquare(in Square square) => Chessboard.Instance.GetTeamOnSquare(in square);
    public void OnPromotion(Piece piece) { }
}
