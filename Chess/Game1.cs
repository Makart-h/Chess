using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Chess.Board;
using Chess.Graphics;
using Chess.Pieces;
using System;
using Chess.AI;
using Chess.Data;
using Chess.Clock;

namespace Chess
{
    internal class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public SpriteBatch SpriteBatch { get => _spriteBatch; }
        public static Game1 Instance;

        private Chessboard chessboard;
        private PieceFactory pieceFactory;
        public Texture2D Overlays { get; private set; }
        private List<Controller> controllers;
        private ChessClock clock;
        private FENObject fenObject;
        private bool draw;
        private bool update;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Instance = this;
            controllers = new List<Controller>();
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
            _graphics.PreferredBackBufferWidth = 576;  // set this value to the desired width of your window
            _graphics.PreferredBackBufferHeight = 576;   // set this value to the desired height of your window
            _graphics.ApplyChanges();

            //"rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            //"6k1/6p1/5p1p/1p2p3/p2p2q1/Pr1P2Nn/Q7/6RK b - - 1 38"
            //"r1bk1q2/pp4pB/4pn2/3p4/3P3Q/2P3P1/PP1N2PP/R5K1 b - - 6 18"
            //5k2/1q6/8/8/4n3/4K3/8/8 w - - 0 1
            try
            {
                fenObject = FENParser.Parse("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
            }
            catch (ArgumentException)
            {
                throw;
            }
            Controller white = new HumanController(Team.White, fenObject.WhitePieces, fenObject.WhiteCastling, fenObject.EnPassantSquare);
            Controller black = new HumanController(Team.Black, fenObject.BlackPieces, fenObject.BlackCastling, fenObject.EnPassantSquare);
            controllers.Add(white);
            controllers.Add(black);
            clock = new ChessClock(white, black, new TimeSpan(0, 10, 0), new TimeSpan(0), fenObject.TeamToMove);
            chessboard.InitilizeBoard(fenObject.AllPieces);
            clock.Start();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            chessboard = new Chessboard(this.Content.Load<Texture2D>("chessboard"));
            pieceFactory = new PieceFactory(this.Content.Load<Texture2D>("Tpieces"));
            Overlays = this.Content.Load<Texture2D>("Tsquares");

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            clock.ActiveController.Update();

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
            foreach (var controller in controllers)
            {
                foreach (var obj in controller.Pieces)
                {
                    _spriteBatch.Draw(obj.Model.RawTexture, obj.Position, obj.Model.TextureRect, obj.Color);
                    if (controller is HumanController hc)
                    {
                        if (hc.DragedPiece.PieceTexture != null)
                        {
                            _spriteBatch.Draw(hc.DragedPiece.PieceTexture.RawTexture, hc.DragedPiece.Position, hc.DragedPiece.PieceTexture.TextureRect, Color.White);
                        }
                    }
                }
            }
            _spriteBatch.End();

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    } 
}
