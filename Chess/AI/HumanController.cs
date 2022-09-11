using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Chess.Board;
using Chess.Movement;
using Chess.Pieces;

namespace Chess.AI
{
    internal class HumanController : Controller
    {
        private MouseState _previousState;
        private Piece _targetedPiece;
        public (Vector2 Position, Graphics.Model PieceTexture) DragedPiece;
        public HumanController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant){}
        public override void Update() => base.Update();
        public override void MakeMove()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && _previousState.LeftButton != ButtonState.Pressed)
            {
                _targetedPiece = Chessboard.Instance.CheckCollisions(mouseState.Position.X, mouseState.Position.Y);
                if (_targetedPiece != null && _targetedPiece.Team == _team)
                {
                    _targetedPiece.IsSelected = true;
                    DragedPiece.PieceTexture = new Graphics.Model(_targetedPiece.Model);
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && _previousState.LeftButton == ButtonState.Pressed)
            {
                if (_targetedPiece != null && _targetedPiece.Team == _team)
                {
                    (int x, int y) = mouseState.Position;
                    if (Chessboard.Instance.MovePiece(_targetedPiece, Chessboard.Instance.FromCords(x, y), out Move move))
                    {
                        OnMoveMade(new MoveMadeEventArgs(this, _targetedPiece, move));
                    }
                    DragedPiece.PieceTexture = null;
                    _targetedPiece.IsSelected = false;
                    _targetedPiece = null;
                }
            }
            if (DragedPiece.PieceTexture != null)
            {
                (int x, int y) = mouseState.Position;
                DragedPiece.Position = new Vector2(x - DragedPiece.PieceTexture.TextureRect.Width / 2, y - DragedPiece.PieceTexture.TextureRect.Height / 2);
            }
            _previousState = mouseState;
        }
    }
}
