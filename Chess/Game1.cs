using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Chess.Board;
using Chess.Graphics;
using Chess.Pieces;
using System;

namespace Chess
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch { get => _spriteBatch; }
        public static Game1 Instance;

        private Chessboard chessboard;
        private PieceFactory pieceFactory;
        public Texture2D Overlays { get; private set; }
        public Texture2D pieces;
        private MouseState previousState;
        private (Square Square, Piece Piece) targetedPiece;
        private (Vector2 Position, Graphics.Model PieceTexture) dragedPiece;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            _graphics.PreferredBackBufferWidth = 576;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 576;   // set this value to the desired height of your window
            _graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            chessboard = new Chessboard(this.Content.Load<Texture2D>("chessboard"));
            pieces = this.Content.Load<Texture2D>("Tpieces");
            pieceFactory = new PieceFactory(pieces);
            Overlays = this.Content.Load<Texture2D>("Tsquares");
            try
            {
                chessboard.InitilizeBoard();
                //chessboard.InitilizeBoard("6k1/6p1/5p1p/1p2p3/p2p2q1/Pr1P2Nn/Q7/6RK b - - 1 38");
                //chessboard.InitilizeBoard("r1bk1q2/pp4pB/4pn2/3p4/3P3Q/2P3P1/PP1N2PP/R5K1 b - - 6 18");
            }
            catch(ArgumentException)
            {
                throw;
            }
            chessboard.Update();

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && previousState.LeftButton != ButtonState.Pressed)
            {
                targetedPiece = chessboard.IsAValidPieceToMove(chessboard.CheckCollisions(mouseState.Position.X, mouseState.Position.Y));
                if (targetedPiece.Piece != null)
                {
                    targetedPiece.Piece.IsSelected = true;
                    dragedPiece.PieceTexture = new Graphics.Model(targetedPiece.Piece.Model);
                }
            }
            else if (mouseState.LeftButton == ButtonState.Released && previousState.LeftButton == ButtonState.Pressed)
            {
                if (targetedPiece.Piece != null && targetedPiece.Piece.Team == chessboard.ToMove)
                {
                    (int x, int y) = mouseState.Position;
                    if (chessboard.MovePiece(targetedPiece, Chessboard.FromCords(x, y)))
                    {
                        chessboard.Update();               
                    }
                    dragedPiece.PieceTexture = null;
                    targetedPiece.Piece.IsSelected = false;
                    targetedPiece.Piece = null;
                }
            }
            if (dragedPiece.PieceTexture != null)
            {
                (int x, int y) = mouseState.Position;
                dragedPiece.Position = new Vector2(x - dragedPiece.PieceTexture.TextureRect.Width/2, y - dragedPiece.PieceTexture.TextureRect.Height / 2);
            }
            previousState = mouseState;
            
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(chessboard.Model.RawTexture, chessboard.Position, chessboard.Model.TextureRect, Color.White);
            List<DrawableObject> overlays = OverlayManager.GetOverlays();
            foreach (var obj in overlays)
            {
                _spriteBatch.Draw(obj.Model.RawTexture, obj.Position, obj.Model.TextureRect, obj.Color);
            }
            List<DrawableObject> objects = chessboard.Pieces;          
            foreach (var obj in objects)
            {
                _spriteBatch.Draw(obj.Model.RawTexture, obj.Position, obj.Model.TextureRect, obj.Color);
            }
            if (dragedPiece.PieceTexture != null)
            {
                _spriteBatch.Draw(dragedPiece.PieceTexture.RawTexture, dragedPiece.Position, dragedPiece.PieceTexture.TextureRect, Color.White);
            }
            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
