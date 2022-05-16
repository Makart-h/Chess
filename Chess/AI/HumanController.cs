using System;
using System.Collections.Generic;
using System.Text;
using Chess.Pieces;
using Chess.Board;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Chess.AI
{
    internal class HumanController : Controller
    {
        private MouseState previousState;
        private (Square Square, Piece Piece) targetedPiece;
        public (Vector2 Position, Graphics.Model PieceTexture) DragedPiece;
        public HumanController(Team team, Piece[] pieces, CastlingRights castlingRights) : base(team, pieces, castlingRights)
        {

        }
        public override void Update()
        {
            base.Update();

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton != ButtonState.Pressed)
            {
                targetedPiece = Chessboard.Instance.IsAValidPieceToMove(Chessboard.Instance.CheckCollisions(mouseState.Position.X, mouseState.Position.Y));
                if (targetedPiece.Piece != null)
                {
                    targetedPiece.Piece.IsSelected = true;
                    DragedPiece.PieceTexture = new Graphics.Model(targetedPiece.Piece.Model);
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                if (targetedPiece.Piece != null && targetedPiece.Piece.Team == team)
                {
                    (int x, int y) = mouseState.Position;
                    if (Chessboard.Instance.MovePiece(targetedPiece, Chessboard.FromCords(x, y)))
                    {
                        Chessboard.Instance.Update();
                    }
                    DragedPiece.PieceTexture = null;
                    targetedPiece.Piece.IsSelected = false;
                    targetedPiece.Piece = null;
                }
            }
            if (DragedPiece.PieceTexture != null)
            {
                (int x, int y) = mouseState.Position;
                DragedPiece.Position = new Vector2(x - DragedPiece.PieceTexture.TextureRect.Width / 2, y - DragedPiece.PieceTexture.TextureRect.Height / 2);
            }
            previousState = mouseState;
        }
        public override void ChooseAMove()
        {
            throw new NotImplementedException();
        }
    }
}
