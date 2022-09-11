using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Chess.Pieces;
using Chess.Board;
using Chess.Movement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chess.AI
{
    internal class HumanController : Controller
    {
        private MouseState previousState;
        private Piece targetedPiece;
        public (Vector2 Position, Graphics.Model PieceTexture) DragedPiece;
        public HumanController(Team team, Piece[] pieces, CastlingRights castlingRights, Square? enPassant) : base(team, pieces, castlingRights, enPassant)
        {

        }
        public override void Update() => base.Update();
        public override void MakeMove()
        {
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton != ButtonState.Pressed)
            {
                targetedPiece = Chessboard.Instance.CheckCollisions(mouseState.Position.X, mouseState.Position.Y);
                if (targetedPiece != null && targetedPiece.Team == _team)
                {
                    targetedPiece.IsSelected = true;
                    DragedPiece.PieceTexture = new Graphics.Model(targetedPiece.Model);
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                if (targetedPiece != null && targetedPiece.Team == _team)
                {
                    (int x, int y) = mouseState.Position;
                    if (Chessboard.Instance.MovePiece(targetedPiece, Chessboard.Instance.FromCords(x, y), out Move move))
                    {
                        OnMoveMade(new MoveMadeEventArgs(this, targetedPiece, move));
                    }
                    DragedPiece.PieceTexture = null;
                    targetedPiece.IsSelected = false;
                    targetedPiece = null;
                }
            }
            if (DragedPiece.PieceTexture != null)
            {
                (int x, int y) = mouseState.Position;
                DragedPiece.Position = new Vector2(x - DragedPiece.PieceTexture.TextureRect.Width / 2, y - DragedPiece.PieceTexture.TextureRect.Height / 2);
            }
            previousState = mouseState;
        }
    }
}
