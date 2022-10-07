using Chess.Board;
using Chess.Graphics;
using Chess.Movement;
using Chess.Pieces;
using Chess.Pieces.Info;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Chess.AI;

internal sealed class HumanController : Controller
{
    private MouseState _previousState;
    private MouseState _currentState;
    private Piece _targetedPiece;
    private DrawableObject _dragedPiece;
    public HumanController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
    {
        _dragedPiece = null;
    }
    public override void Update() => base.Update();
    public override DrawableObject[] GetDrawableObjects()
    {
        List<DrawableObject> drawables = new(_pieces);
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
            var mouse = new Vector2(x - _dragedPiece.DestinationRectangle.Width / 2, y - _dragedPiece.DestinationRectangle.Height / 2);
            Vector2 offset = mouse - _dragedPiece.Position;
            _dragedPiece.MoveObject(offset);
        }
    }
    private void CheckIfPieceIsTargeted()
    {
        (int x, int y) = Chess.Instance.TranslateToBoard(_currentState.Position);
        _targetedPiece = Chessboard.Instance.CheckCollisions(x, y);
        if (_targetedPiece != null && _targetedPiece.Team == Team)
        {
            _targetedPiece.IsSelected = true;
            _dragedPiece ??= new DrawableObject(_targetedPiece.Model, new Rectangle(x, y, Chessboard.Instance.SquareSideLength, Chessboard.Instance.SquareSideLength));
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
