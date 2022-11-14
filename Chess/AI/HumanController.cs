using Chess.Board;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using IDrawable = Chess.Graphics.IDrawable;

namespace Chess.AI;

internal sealed class HumanController : Controller
{
    private MouseState _previousState;
    private MouseState _currentState;
    private Piece _targetedPiece;
    private IMovableDrawable _dragedPiece;
    public HumanController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
    {
        _dragedPiece = null;
    }
    public override void Update() => base.Update();
    public override IEnumerable<IDrawable> GetDrawableObjects()
    {
        List<IDrawable> drawables = new(_piecesModels.Values);
        if (_dragedPiece != null)
            drawables.Add(_dragedPiece);
        return drawables.ToArray();
    }
    public override void MakeMove()
    {
        _currentState = Mouse.GetState();

        if (_currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton != ButtonState.Pressed)
            CheckIfPieceIsTargeted();

        else if (_currentState.LeftButton == ButtonState.Released && _previousState.LeftButton == ButtonState.Pressed)
            ApplyMove();

        UpdateDragedPiece();
        _previousState = _currentState;
    }
    private void UpdateDragedPiece()
    {
        if (_dragedPiece != null)
        {
            (int x, int y) = Chess.Instance.TranslateToBoard(_currentState.Position);
            var mouse = new Vector2(x - _dragedPiece.DestinationRect.Width / 2, y - _dragedPiece.DestinationRect.Height / 2);
            Vector2 offset = mouse - _dragedPiece.Position;
            _dragedPiece.Position += offset;
        }
    }
    private void CheckIfPieceIsTargeted()
    {
        (int x, int y) = Chess.Instance.TranslateToBoard(_currentState.Position);
        _targetedPiece = Chessboard.Instance.CheckCollisions(x, y);
        if (_targetedPiece != null && _targetedPiece.Team == Team)
        {
            _targetedPiece.IsSelected = true;         
            _dragedPiece ??= PieceFactory.CreatePieceModel(_targetedPiece);
        }
    }
    private void ApplyMove()
    {
        if (_targetedPiece != null && _targetedPiece.Team == Team)
        {
            (int x, int y) = Chess.Instance.TranslateToBoard(_currentState.Position);

            if (Chessboard.Instance.MovePiece(_targetedPiece, Chessboard.Instance.FromCords(x, y), out Move move))
                OnMoveMade(new MoveMadeEventArgs(this, _targetedPiece, move));
            
            _dragedPiece = null;
            _targetedPiece.IsSelected = false;
            _targetedPiece = null;
        }
    }
}
