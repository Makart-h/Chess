using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Chess.Board;

namespace Chess.Graphics
{
    class SquareOverlay : DrawableObject
    {
        public SquareOverlayType Type { get; private set; }
        public SquareOverlay(SquareOverlayType type, Square square)
        {
            this.Type = type;
            this.model = new Model(Game1.Instance.Overlays, Square.SquareWidth * (int)type, 0, Square.SquareWidth, Square.SquareHeight);
            this.position = Chessboard.Instance.ToCordsFromSquare(square);
        }
        public void Move(Square square)
        {
            this.position = Chessboard.Instance.ToCordsFromSquare(square);
        }
    }
}
